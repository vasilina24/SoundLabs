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
public class RepairsController(SoundLabsContext db) : ControllerBase
{
    private static readonly string[] MasterStatuses = ["В работе", "Ожидает запчасти", "Готов к выдаче"];
    private static readonly string[] ManagerStatuses = ["Принят", "В работе", "Ожидает запчасти", "Готов к выдаче", "Выполнен"];
    private static IQueryable<RepairDto> Project(IQueryable<Repair> q) =>
    q.Select(r => new RepairDto(r.IdRepair, r.CreatedDate, r.InstrumentType, r.InstrumentModel,
    r.FaultDescription, r.Status, r.StatusDate, r.PlannedDate, r.CompletedDate, r.TotalCost,
    r.IdClient, r.IdClientNavigation.FullName, r.IdService, r.IdServiceNavigation.ServiceName,
    r.IdMaster, r.IdMasterNavigation != null ? r.IdMasterNavigation.FullName : null,
    r.UsedParts.Select(u => new UsedPartDto(u.IdPart, u.IdPartNavigation.PartName,
    u.Quantity, u.UnitPrice)).ToList()));
    /// <summary>Список мастеров для назначения заявок менеджером.</summary>
    [HttpGet("masters")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<List<RepairMasterDto>>> GetMasters()
    {
        return await db.Employees.AsNoTracking()
            .Where(e => e.IdUserNavigation.IdRoleNavigation.RoleName == Roles.Master)
            .OrderBy(e => e.FullName)
            .Select(e => new RepairMasterDto(e.IdEmployee, e.FullName, e.JobTitle))
            .ToListAsync();
    }

    /// <summary>Покупатель – свои заявки, мастер – назначенные ему, менеджер – все.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Client + "," + Roles.Manager + "," + Roles.Master)]
    public async Task<ActionResult<List<RepairDto>>> GetAll([FromQuery] string? status)
    {
        var q = db.Repairs.AsNoTracking().AsQueryable();
        switch (User.GetRole())
        {
            case Roles.Client:
                var clientId = await CurrentClientIdAsync();
                q = q.Where(r => r.IdClient == clientId);
                break;
            case Roles.Master:
                var masterId = await CurrentEmployeeIdAsync();
                q = q.Where(r => r.IdMaster == masterId);
                break;
        }
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(r => r.Status == status);
        return await Project(q.OrderByDescending(r => r.CreatedDate)).ToListAsync();

    }
    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Client + "," + Roles.Manager + "," + Roles.Master)]
    public async Task<ActionResult<RepairDto>> GetById(int id)
    {
        var dto = await Project(db.Repairs.AsNoTracking().Where(r => r.IdRepair == id)).FirstOrDefaultAsync();
        if (dto is null) return NotFound();
        var allowed = User.GetRole() switch
        {
            Roles.Client => dto.ClientId == await CurrentClientIdAsync(),
            Roles.Master => dto.MasterId == await CurrentEmployeeIdAsync(),
            _ => true
        };
        return allowed ? dto : Forbid();
    }
    /// <summary>Заявка на ремонт: покупатель подаёт сам, менеджер – по обращению клиента.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Client + "," + Roles.Manager)]
    public async Task<ActionResult<RepairDto>> Create(CreateRepairDto dto)
    {
        int? clientId = User.GetRole() == Roles.Client ? await CurrentClientIdAsync() : dto.ClientId;
        if (clientId is null) return BadRequest(new { message = "Не указан клиент" });
        if (!await db.Services.AnyAsync(s => s.IdService == dto.ServiceId))
            return BadRequest(new { message = "Услуга не найдена" });
        var repair = new Repair
        {
            IdClient = clientId.Value,
            IdService = dto.ServiceId,
            InstrumentType = dto.InstrumentType,
            InstrumentModel = dto.InstrumentModel,
            FaultDescription = dto.FaultDescription,
            Status = "Новая" // стоимость услуги подставит триггер
        };
        db.Repairs.Add(repair);
        await db.SaveChangesAsync();
        var result = await Project(db.Repairs.AsNoTracking().Where(r => r.IdRepair == repair.IdRepair)).SingleAsync();
        return CreatedAtAction(nameof(GetById), new { id = repair.IdRepair }, result);
    }
    /// <summary>Назначение мастера и срока выполнения (менеджер).</summary>
    [HttpPatch("{id:int}/assign")]
    [Authorize(Roles = Roles.Manager)]

    public async Task<IActionResult> Assign(int id, AssignRepairDto dto)
    {
        var repair = await db.Repairs.FindAsync(id);
        if (repair is null) return NotFound();
        var isMaster = await db.Employees.AnyAsync(e =>
        e.IdEmployee == dto.MasterId && e.IdUserNavigation.IdRoleNavigation.RoleName == Roles.Master);
        if (!isMaster) return BadRequest(new { message = "Указанный сотрудник не является мастером" });
        if (dto.PlannedDate is { } d && d < DateOnly.FromDateTime(repair.CreatedDate))
            return BadRequest(new { message = "Плановый срок не может быть раньше даты создания заказа" });
        repair.IdMaster = dto.MasterId;
        repair.PlannedDate = dto.PlannedDate;
        if (repair.Status == "Новая") repair.Status = "Принят";
        await db.SaveChangesAsync();
        return NoContent();
    }
    /// <summary>Смена статуса: мастер – по своим заказам, менеджер – любые.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Roles.Master + "," + Roles.Manager)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeRepairStatusDto dto)
    {
        var repair = await db.Repairs.FindAsync(id);
        if (repair is null) return NotFound();
        if (User.GetRole() == Roles.Master)
        {
            if (repair.IdMaster != await CurrentEmployeeIdAsync()) return Forbid();
            if (!MasterStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Мастер может указать: «В работе», «Ожидает запчасти», «Готов к выдаче»" });
        }
        else if (!ManagerStatuses.Contains(dto.Status))
            return BadRequest(new { message = "Недопустимый статус" });
        if (repair.Status == "Выполнен")
            return BadRequest(new { message = "Заказ уже выполнен" });
        repair.Status = dto.Status; // status_date и completed_date выставляет триггер
        await db.SaveChangesAsync();
        return NoContent();
    }
    /// <summary>Мастер добавляет запчасть; остаток склада и стоимость ремонта обновляются в одной транзакции.</summary>
    [HttpPost("{id:int}/parts")]
    [Authorize(Roles = Roles.Master)]

    public async Task<ActionResult<RepairDto>> AddPart(int id, AddPartDto dto)
    {
        var repair = await db.Repairs.FindAsync(id);
        if (repair is null) return NotFound();
        if (repair.IdMaster != await CurrentEmployeeIdAsync()) return Forbid();
        if (repair.Status is "Выполнен" or "Готов к выдаче")
            return BadRequest(new { message = "Нельзя вносить запчасти в завершённый ремонт" });
        if (dto.Quantity <= 0) return BadRequest(new { message = "Количество должно быть больше нуля." });
        await using var tx = await db.Database.BeginTransactionAsync();
        var part = await db.Parts.FirstOrDefaultAsync(p => p.IdPart == dto.PartId);
        if (part is null) return BadRequest(new { message = "Запчасть не найдена" });
        if (part.StockQuantity < dto.Quantity)
            return BadRequest(new { message = $"Недостаточно запчастей на складе. Доступно: {part.StockQuantity} шт." });
        var existing = await db.UsedParts.FirstOrDefaultAsync(x => x.IdRepair == id && x.IdPart == dto.PartId);
        if (existing is null)
            db.UsedParts.Add(new UsedPart { IdRepair = id, IdPart = dto.PartId, Quantity = dto.Quantity, UnitPrice = part.Price });
        else
        {
            existing.Quantity += dto.Quantity;
            existing.UnitPrice = part.Price;
        }
        part.StockQuantity -= dto.Quantity;
        repair.TotalCost += part.Price * dto.Quantity;
        db.EventLogs.Add(new EventLog { IdUser = User.GetUserId(), EventType = "Использование запчасти", Description = $"В ремонте №{id} использовано: {part.PartName}, количество: {dto.Quantity}." });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return await Project(db.Repairs.AsNoTracking().Where(r => r.IdRepair == id)).SingleAsync();
    }
    [HttpGet("parts")]
    [Authorize(Roles = Roles.Master)]
    public async Task<IActionResult> GetAvailableParts() => Ok(await db.Parts.AsNoTracking()
        .Where(p => p.StockQuantity > 0).OrderBy(p => p.PartName)
        .Select(p => new { id = p.IdPart, name = p.PartName, article = p.Article, price = p.Price, stock = p.StockQuantity, minimum = p.MinQuantity }).ToListAsync());

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
