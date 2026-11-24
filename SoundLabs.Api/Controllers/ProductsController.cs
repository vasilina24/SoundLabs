using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundLabs.Dtos;
using SoundLabs.Infrastructure;
using SoundLabs.Models;

namespace SoundLabs.Controllers;
[ApiController]
public class ProductsController(SoundLabsContext db, IWebHostEnvironment environment) : ControllerBase
{
    // ---------- Каталог (доступен всем, в том числе неавторизованным) ----------
    [HttpGet("api/products")]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var q = db.Products.AsNoTracking().AsQueryable();
        if (categoryId is not null) q = q.Where(p => p.IdCategory == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(p => p.ProductName.ToLower().Contains(s) || p.Article.ToLower().Contains(s));
        }
        return await q.OrderBy(p => p.ProductName)
        .Select(p => new ProductDto(p.IdProduct, p.Article, p.ProductName, p.Description, p.Price,
        p.StockQuantity - p.ReservedQuantity, p.IdCategory, p.IdCategoryNavigation.CategoryName, p.ImageUrl))
        .ToListAsync();
    }
    [HttpGet("api/products/{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var dto = await db.Products.AsNoTracking().Where(p => p.IdProduct == id)
        .Select(p => new ProductDto(p.IdProduct, p.Article, p.ProductName, p.Description, p.Price,
        p.StockQuantity - p.ReservedQuantity, p.IdCategory, p.IdCategoryNavigation.CategoryName, p.ImageUrl))
        .FirstOrDefaultAsync();
        return dto is null ? NotFound() : dto;
    }
    [HttpGet("api/categories")]
    public async Task<IActionResult> Categories() =>
    Ok(await db.Categories.AsNoTracking()

    .Select(c => new { id = c.IdCategory, name = c.CategoryName, description = c.Description })
    .ToListAsync());
    [HttpGet("api/services")]
    public async Task<IActionResult> Services() =>
    Ok(await db.Services.AsNoTracking()
    .Select(s => new {
        id = s.IdService,
        name = s.ServiceName,
        description = s.Description,
        price = s.Price,
        durationDays = s.DurationDays
    })
    .ToListAsync());
    // ---------- Управление ассортиментом (менеджер) ----------
    [HttpPost("api/products")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<ProductDto>> Create(ProductSaveDto dto)
    {
        if (dto.CategoryId <= 0 || !await db.Categories.AnyAsync(c => c.IdCategory == dto.CategoryId))
            return BadRequest(new { message = "Выберите существующую категорию товара" });
        if (string.IsNullOrWhiteSpace(dto.Article) || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Артикул и название товара обязательны" });

        var p = new Product();
        Apply(p, dto);
        db.Products.Add(p);
        await db.SaveChangesAsync();
        var result = new ProductDto(p.IdProduct, p.Article, p.ProductName, p.Description,
            p.Price, p.StockQuantity - p.ReservedQuantity, p.IdCategory,
            await db.Categories.Where(c => c.IdCategory == p.IdCategory).Select(c => c.CategoryName).SingleAsync(), p.ImageUrl);
        return CreatedAtAction(nameof(GetById), new { id = p.IdProduct }, result);
    }
    [HttpPut("api/products/{id:int}")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Update(int id, ProductSaveDto dto)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        if (dto.CategoryId <= 0 || !await db.Categories.AnyAsync(c => c.IdCategory == dto.CategoryId))
            return BadRequest(new { message = "Выберите существующую категорию товара" });
        if (string.IsNullOrWhiteSpace(dto.Article) || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Артикул и название товара обязательны" });
        Apply(p, dto);
        await db.SaveChangesAsync();
        return NoContent();
    }
    [HttpPost("api/products/{id:int}/image")]
    [Authorize(Roles = Roles.Manager)]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound(new { message = "Товар не найден" });
        if (file is null || file.Length == 0) return BadRequest(new { message = "Выберите изображение" });
        if (file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "Размер изображения не должен превышать 5 МБ" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new Dictionary<string, (string ContentType, byte[] Signature)>
        {
            [".jpg"] = ("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF }),
            [".jpeg"] = ("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF }),
            [".png"] = ("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
            [".webp"] = ("image/webp", new byte[] { 0x52, 0x49, 0x46, 0x46 })
        };
        if (!allowed.TryGetValue(extension, out var format) || file.ContentType != format.ContentType)
            return BadRequest(new { message = "Поддерживаются только JPG, PNG и WEBP" });

        var header = new byte[extension == ".webp" ? 12 : format.Signature.Length];
        await using (var input = file.OpenReadStream())
        {
            var read = await input.ReadAsync(header);
            var validHeader = read == header.Length && header.Take(format.Signature.Length).SequenceEqual(format.Signature);
            if (extension == ".webp")
                validHeader = validHeader && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P';
            if (!validHeader)
                return BadRequest(new { message = "Файл не является корректным изображением выбранного формата" });
        }

        var folder = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "products");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, fileName);
        await using (var output = System.IO.File.Create(fullPath))
        await using (var input = file.OpenReadStream())
            await input.CopyToAsync(output);

        var oldImage = product.ImageUrl;
        product.ImageUrl = $"/uploads/products/{fileName}";
        await db.SaveChangesAsync();
        DeleteLocalImage(oldImage);
        return Ok(new { imageUrl = product.ImageUrl });
    }

    [HttpDelete("api/products/{id:int}")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        var oldImage = p.ImageUrl;
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        DeleteLocalImage(oldImage); // триггер запретит удаление товара из незавершённых заказов
        return NoContent();
    }
    private void DeleteLocalImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/products/", StringComparison.Ordinal)) return;
        var fileName = Path.GetFileName(imageUrl);
        var root = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var path = Path.Combine(root, "uploads", "products", fileName);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private static void Apply(Product p, ProductSaveDto dto)
    {

        p.Article = dto.Article;
        p.ProductName = dto.Name;
        p.Description = dto.Description;
        p.Price = dto.Price;
        p.StockQuantity = dto.StockQuantity;
        p.MinQuantity = dto.MinQuantity;
        p.IdCategory = dto.CategoryId;
    }
}