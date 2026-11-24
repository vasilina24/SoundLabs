using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundLabs.Dtos;
using SoundLabs.Infrastructure;
using SoundLabs.Models;
namespace SoundLabs.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(SoundLabsContext db) : ControllerBase
{
    // Переходы зависят от способа получения заказа.
    // Доставка: Новый -> Подтверждён -> В пути -> Доставлен.
    // Самовывоз: Новый -> Подтверждён -> Готов к выдаче -> Выполнен.
    private static string[] AllowedNextStatuses(Order order) => order.Status switch
    {
        "Новый" => ["Подтверждён", "Отклонён"],
        "Подтверждён" when order.ReceiveMethod == "Доставка" => ["В пути", "Отклонён"],
        "Подтверждён" => ["Готов к выдаче", "Отклонён"],
        "В пути" => ["Доставлен"],
        "Готов к выдаче" => ["Выполнен"],
        _ => []
    };
    private static readonly string[] Methods = ["Самовывоз", "Доставка"];
    private static IQueryable<OrderDto> Project(IQueryable<Order> q) =>
    q.Select(o => new OrderDto(o.IdOrder, o.OrderDate, o.Status, o.ReceiveMethod,o.DeliveryAddress, o.TotalAmount,
    o.RejectReason, o.StatusDate, o.IdClient, o.IdClientNavigation.FullName,
    o.OrderItems.Select(i => new OrderItemDto(i.IdProduct, i.IdProductNavigation.ProductName,
    i.Quantity, i.UnitPrice)).ToList()));
    /// <summary>Покупатель видит свои заказы, менеджер – все.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Client + "," + Roles.Manager)]
    public async Task<ActionResult<List<OrderDto>>> GetAll([FromQuery] string? status)
    {
        var q = db.Orders.AsNoTracking().AsQueryable();
        if (User.GetRole() == Roles.Client)
        {
            var clientId = await CurrentClientIdAsync();
            q = q.Where(o => o.IdClient == clientId);
        }
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
        return await Project(q.OrderByDescending(o => o.OrderDate)).ToListAsync();

    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Client + "," + Roles.Manager)]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var dto = await Project(db.Orders.AsNoTracking().Where(o => o.IdOrder == id)).FirstOrDefaultAsync();
        if (dto is null) return NotFound();
        if (User.GetRole() == Roles.Client && dto.ClientId != await CurrentClientIdAsync())
            return Forbid();
        return dto;
    }
    /// <summary>Уведомления покупателя об изменении статусов его заказов.</summary>
    [HttpGet("notifications")]
    [Authorize(Roles = Roles.Client)]
    public async Task<IActionResult> Notifications()
    {
        var userId = User.GetUserId();
        var items = await db.EventLogs.AsNoTracking()
            .Where(e => e.IdUser == userId && e.EventType == "Статус заказа изменён")
            .OrderByDescending(e => e.EventDate)
            .Take(10)
            .Select(e => new { id = e.IdLog, createdAt = e.EventDate, message = e.Description })
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>Оформление заказа покупателем (статус «Новый»).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Client)]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
    {
        if (!Methods.Contains(dto.ReceiveMethod))
            return BadRequest(new { message = "Способ получения: «Самовывоз» или «Доставка»" });
        if (dto.Items.Count == 0)
            return BadRequest(new { message = "Заказ не содержит позиций" });
        var clientId = await CurrentClientIdAsync();
        if (clientId is null) return BadRequest(new { message = "Карточка клиента не найдена" });
        // одинаковые товары объединяем в одну позицию
        var items = dto.Items.GroupBy(i => i.ProductId)
        .Select(g => new OrderItem { IdProduct = g.Key, Quantity = g.Sum(x => x.Quantity) })
        .ToList();
        var ids = items.Select(i => i.IdProduct).ToList();
        if (dto.ReceiveMethod == "Доставка" && string.IsNullOrWhiteSpace(dto.DeliveryAddress))
        {
            return BadRequest(new { message = "Укажите адрес доставки" });
        }
        if (await db.Products.CountAsync(p => ids.Contains(p.IdProduct)) != ids.Count)
            return BadRequest(new { message = "Один или несколько товаров не найдены" });
        var order = new Order
        {
            IdClient = clientId.Value,
            ReceiveMethod = dto.ReceiveMethod,
            DeliveryAddress = dto.ReceiveMethod == "Доставка"? dto.DeliveryAddress!.Trim() : null,
            Status = "Новый",
            OrderItems = items // цену и проверку остатка выполняет триггер БД
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var result = await Project(db.Orders.AsNoTracking().Where(o => o.IdOrder == order.IdOrder)).SingleAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.IdOrder }, result);
    }
    /// <summary>Подтверждение / отклонение / выдача заказа (менеджер).</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeOrderStatusDto dto)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        var newStatus = dto.Status?.Trim();
        var allowed = AllowedNextStatuses(order);
        if (string.IsNullOrWhiteSpace(newStatus) || !allowed.Contains(newStatus, StringComparer.Ordinal))
            return BadRequest(new { message = $"Недопустимый переход: «{order.Status}» → «{newStatus ?? "(пусто)"}». Допустимые статусы: {(allowed.Length == 0 ? "нет" : string.Join(", ", allowed))}." });
        if (newStatus == "Отклонён")
        {
            if (string.IsNullOrWhiteSpace(dto.RejectReason))
                return BadRequest(new { message = "Укажите причину отклонения" });
            order.RejectReason = dto.RejectReason.Trim();
        }
        order.Status = newStatus;
        // В БД поле status_date имеет тип timestamp without time zone.
        // Поэтому используем локальное DateTime без Kind=UTC.
        order.StatusDate = DateTime.Now;
        order.IdEmployee = await CurrentEmployeeIdAsync();

        var clientUserId = await db.Clients
            .Where(c => c.IdClient == order.IdClient)
            .Select(c => c.IdUser)
            .SingleAsync();
        db.EventLogs.Add(new EventLog
        {
            IdUser = clientUserId,
            EventType = "Статус заказа изменён",
            Description = $"Статус заказа №{order.IdOrder} изменён на «{order.Status}»." 
        });

        await db.SaveChangesAsync();
        return NoContent();
    }
    private Task<int?> CurrentClientIdAsync()
    {
        var uid = User.GetUserId();
        return db.Clients.Where(c => c.IdUser == uid).Select(c => (int?)c.IdClient).FirstOrDefaultAsync();
    }
    private Task<int?> CurrentEmployeeIdAsync()
    {
        var uid = User.GetUserId();
        return db.Employees.Where(e => e.IdUser == uid).Select(e => (int?)e.IdEmployee).FirstOrDefaultAsync();
    }
}