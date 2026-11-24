using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundLabs.Infrastructure;
using SoundLabs.Models;

namespace SoundLabs.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = Roles.Manager)]
public class InventoryController(SoundLabsContext db) : ControllerBase
{
    public record PartSave(string Article, string Name, decimal Price, int StockQuantity, int MinQuantity);
    public record SupplierSave(string Name, string ContactPerson, string Phone, string Email, string? Address);
    public record PurchaseLine(int PartId, int Quantity, decimal UnitPrice);
    public record PurchaseCreate(int SupplierId, List<PurchaseLine> Items);

    [HttpGet("parts")]
    public async Task<IActionResult> Parts() => Ok(await db.Parts.AsNoTracking().OrderBy(x => x.PartName)
        .Select(x => new { id = x.IdPart, article = x.Article, name = x.PartName, price = x.Price, stock = x.StockQuantity, minimum = x.MinQuantity }).ToListAsync());

    [HttpPost("parts")]
    public async Task<IActionResult> CreatePart(PartSave dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Article) || string.IsNullOrWhiteSpace(dto.Name) || dto.Price < 0 || dto.StockQuantity < 0 || dto.MinQuantity < 0) return BadRequest(new { message = "Проверьте данные запчасти." });
        var p = new Part { Article = dto.Article.Trim(), PartName = dto.Name.Trim(), Price = dto.Price, StockQuantity = dto.StockQuantity, MinQuantity = dto.MinQuantity };
        db.Parts.Add(p); db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Добавление запчасти", Description = $"Добавлена запчасть {p.PartName} ({p.Article})." }); await db.SaveChangesAsync(); return Ok(new { id = p.IdPart });
    }

    [HttpPut("parts/{id:int}")]
    public async Task<IActionResult> UpdatePart(int id, PartSave dto)
    {
        var p = await db.Parts.FindAsync(id); if (p is null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Article) || string.IsNullOrWhiteSpace(dto.Name) || dto.Price < 0 || dto.StockQuantity < 0 || dto.MinQuantity < 0) return BadRequest(new { message = "Проверьте данные запчасти." });
        p.Article = dto.Article.Trim(); p.PartName = dto.Name.Trim(); p.Price = dto.Price; p.StockQuantity = dto.StockQuantity; p.MinQuantity = dto.MinQuantity;
        db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Изменение запчасти", Description = $"Изменены данные запчасти {p.PartName}." });
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers() => Ok(await db.Suppliers.AsNoTracking().OrderBy(x => x.SupplierName)
        .Select(x => new { id = x.IdSupplier, name = x.SupplierName, contact = x.ContactPerson, phone = x.Phone, email = x.Email, address = x.Address }).ToListAsync());

    [HttpPost("suppliers")]
    public async Task<IActionResult> CreateSupplier(SupplierSave dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.ContactPerson) || dto.Phone?.Length != 11 || !dto.Phone.All(char.IsDigit)) return BadRequest(new { message = "Заполните название, контактное лицо и телефон из 11 цифр." });
        db.Suppliers.Add(new Supplier { SupplierName = dto.Name.Trim(), ContactPerson = dto.ContactPerson.Trim(), Phone = dto.Phone, Email = dto.Email?.Trim() ?? "", Address = dto.Address?.Trim() });
        db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Добавление поставщика", Description = $"Добавлен поставщик {dto.Name.Trim()}." });
        await db.SaveChangesAsync(); return Ok();
    }

    [HttpPut("suppliers/{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, SupplierSave dto)
    {
        var s = await db.Suppliers.FindAsync(id); if (s is null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.ContactPerson) || dto.Phone?.Length != 11 || !dto.Phone.All(char.IsDigit)) return BadRequest(new { message = "Проверьте данные поставщика." });
        s.SupplierName = dto.Name.Trim(); s.ContactPerson = dto.ContactPerson.Trim(); s.Phone = dto.Phone; s.Email = dto.Email?.Trim() ?? ""; s.Address = dto.Address?.Trim();
        db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Изменение поставщика", Description = $"Изменены данные поставщика {s.SupplierName}." });
        await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("suppliers/{id:int}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var s = await db.Suppliers.FindAsync(id); if (s is null) return NotFound();
        if (await db.SupplierOrders.AnyAsync(x => x.IdSupplier == id)) return Conflict(new { message = "Нельзя удалить поставщика: с ним связаны заказы поставщикам." });
        db.Suppliers.Remove(s); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpGet("purchases")]
    public async Task<IActionResult> Purchases() => Ok(await db.SupplierOrders.AsNoTracking().Include(x => x.IdSupplierNavigation).Include(x => x.SupplierOrderItems).ThenInclude(x => x.IdPartNavigation)
        .OrderByDescending(x => x.OrderDate).Select(x => new { id = x.IdSupplierOrder, supplier = x.IdSupplierNavigation.SupplierName, date = x.OrderDate, status = x.Status, total = x.TotalAmount,
            items = x.SupplierOrderItems.Select(i => new { partId = i.IdPart, partName = i.IdPartNavigation != null ? i.IdPartNavigation.PartName : "", quantity = i.Quantity, received = i.ReceivedQuantity, unitPrice = i.UnitPrice }) }).ToListAsync());

    [HttpPost("purchases")]
    public async Task<IActionResult> CreatePurchase(PurchaseCreate dto)
    {
        if (dto.Items is null || dto.Items.Count == 0 || !await db.Suppliers.AnyAsync(x => x.IdSupplier == dto.SupplierId)) return BadRequest(new { message = "Выберите поставщика и добавьте хотя бы одну запчасть." });
        var employeeId = await db.Employees.Where(e => e.IdUser == User.GetUserId()).Select(e => (int?)e.IdEmployee).SingleOrDefaultAsync();
        if (employeeId is null) return BadRequest(new { message = "Профиль сотрудника не найден." });
        var ids = dto.Items.Select(x => x.PartId).Distinct().ToList();
        var parts = await db.Parts.Where(p => ids.Contains(p.IdPart)).ToDictionaryAsync(p => p.IdPart);
        if (dto.Items.Any(x => x.Quantity <= 0 || x.UnitPrice < 0 || !parts.ContainsKey(x.PartId))) return BadRequest(new { message = "Проверьте состав заказа поставщику." });
        var order = new SupplierOrder { IdSupplier = dto.SupplierId, IdEmployee = employeeId.Value, OrderDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), Status = "Оформлен", TotalAmount = dto.Items.Sum(x => x.Quantity * x.UnitPrice) };
        foreach (var item in dto.Items) order.SupplierOrderItems.Add(new SupplierOrderItem { IdPart = item.PartId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
        db.SupplierOrders.Add(order); db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Заказ поставщику", Description = $"Оформлен заказ поставщику {order.IdSupplier} на сумму {order.TotalAmount}." }); await db.SaveChangesAsync(); return Ok(new { id = order.IdSupplierOrder });
    }

    [HttpPost("purchases/{orderId:int}/receive/{partId:int}")]
    public async Task<IActionResult> Receive(int orderId, int partId, [FromBody] int quantity)
    {
        if (quantity <= 0) return BadRequest(new { message = "Количество должно быть положительным." });
        await using var tx = await db.Database.BeginTransactionAsync();
        var order = await db.SupplierOrders.Include(x => x.SupplierOrderItems).FirstOrDefaultAsync(x => x.IdSupplierOrder == orderId);
        if (order is null) return NotFound();
        var item = order.SupplierOrderItems.FirstOrDefault(x => x.IdPart == partId); if (item is null) return NotFound();
        var remaining = item.Quantity - item.ReceivedQuantity;
        if (quantity > remaining) return BadRequest(new { message = $"Можно принять не более {remaining} шт." });
        var part = await db.Parts.FirstAsync(x => x.IdPart == partId);
        item.ReceivedQuantity += quantity; part.StockQuantity += quantity;
        order.Status = order.SupplierOrderItems.All(x => x.ReceivedQuantity == x.Quantity) ? "Получен" : "Получен частично";
        db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Приёмка поставки", Description = $"Принято {quantity} шт. запчасти {part.PartName} по заказу поставщику №{orderId}." });
        await db.SaveChangesAsync(); await tx.CommitAsync(); return NoContent();
    }
}
