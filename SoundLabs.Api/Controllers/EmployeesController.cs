using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoundLabs.Dtos;
using SoundLabs.Infrastructure;
using SoundLabs.Models;

namespace SoundLabs.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize(Roles = Roles.Admin)]
public class EmployeesController(SoundLabsContext db) : ControllerBase
{
    private static readonly string[] AllowedRoles = [Roles.Manager, Roles.Master];

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await db.Employees.AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeDto(e.IdEmployee, e.IdUserNavigation.Login, e.FullName, e.Phone, e.Email,
                e.IdUserNavigation.IdRoleNavigation.RoleName, e.JobTitle, e.HireDate, e.IdUserNavigation.IsActive))
            .ToListAsync();
        return employees;
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeDto dto)
    {
        var role = dto.Role.Trim();
        if (!AllowedRoles.Contains(role)) return BadRequest(new { message = "Можно назначить только роль «Менеджер» или «Мастер»." });
        var login = dto.Login.Trim(); var email = dto.Email.Trim();
        if (await db.Users.AnyAsync(u => u.Login == login)) return Conflict(new { message = "Этот логин уже занят." });
        if (await db.Clients.AnyAsync(c => c.Email == email) || await db.Employees.AnyAsync(e => e.Email == email))
            return Conflict(new { message = "Этот адрес электронной почты уже используется." });
        var roleId = await db.Roles.Where(r => r.RoleName == role).Select(r => r.IdRole).SingleOrDefaultAsync();
        if (roleId == 0) return BadRequest(new { message = "Выбранная роль отсутствует в базе данных." });
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = new User { Login = login, Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), IdRole = roleId, IsActive = true };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var employee = new Employee { IdUser = user.IdUser, FullName = dto.FullName.Trim(), Phone = dto.Phone.Trim(), Email = email, JobTitle = role, HireDate = DateOnly.FromDateTime(DateTime.Today) };
        db.Employees.Add(employee);
        db.EventLogs.Add(new EventLog { IdUser = user.IdUser, EventType = "Создание сотрудника", Description = $"Администратор создал учётную запись сотрудника {login} ({role})." });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return CreatedAtAction(nameof(GetAll), new { }, new EmployeeDto(employee.IdEmployee, user.Login, employee.FullName, employee.Phone, employee.Email, role, employee.JobTitle, employee.HireDate, user.IsActive));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        var employee = await db.Employees.Include(e => e.IdUserNavigation).ThenInclude(u => u.IdRoleNavigation).FirstOrDefaultAsync(e => e.IdEmployee == id);
        if (employee is null) return NotFound();
        var role = dto.Role.Trim(); var login = dto.Login.Trim(); var email = dto.Email.Trim();
        if (!AllowedRoles.Contains(role)) return BadRequest(new { message = "Можно назначить только роль «Менеджер» или «Мастер»." });
        if (await db.Users.AnyAsync(u => u.Login == login && u.IdUser != employee.IdUser)) return Conflict(new { message = "Этот логин уже занят." });
        if (await db.Clients.AnyAsync(c => c.Email == email && c.IdUser != employee.IdUser) || await db.Employees.AnyAsync(e => e.Email == email && e.IdEmployee != id))
            return Conflict(new { message = "Этот адрес электронной почты уже используется." });
        var roleId = await db.Roles.Where(r => r.RoleName == role).Select(r => r.IdRole).SingleOrDefaultAsync();
        if (roleId == 0) return BadRequest(new { message = "Выбранная роль отсутствует в базе данных." });
        employee.FullName = dto.FullName.Trim(); employee.Phone = dto.Phone.Trim(); employee.Email = email; employee.JobTitle = role;
        employee.IdUserNavigation.Login = login; employee.IdUserNavigation.IdRole = roleId;
        if (!string.IsNullOrWhiteSpace(dto.Password)) employee.IdUserNavigation.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        db.EventLogs.Add(new EventLog { IdUser = employee.IdUser, EventType = "Изменение сотрудника", Description = $"Изменены данные сотрудника {login}; роль: {role}." });
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Удаление выполняется мягко: сохраняем историю заказов и ремонтов, но запрещаем вход.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var employee = await db.Employees.Include(e => e.IdUserNavigation).FirstOrDefaultAsync(e => e.IdEmployee == id);
        if (employee is null) return NotFound();
        if (employee.IdUser == User.GetUserId()) return BadRequest(new { message = "Нельзя отключить собственную учётную запись." });
        var activeRepairs = await db.Repairs.AnyAsync(r => r.IdMaster == id && r.Status != "Выполнен" && r.Status != "Отклонён");
        if (activeRepairs) return Conflict(new { message = "Нельзя удалить сотрудника: за ним закреплены незавершённые ремонты. Сначала переназначьте их." });
        var activeOrders = await db.Orders.AnyAsync(o => o.IdEmployee == id && o.Status != "Выполнен" && o.Status != "Доставлен" && o.Status != "Отклонён");
        if (activeOrders) return Conflict(new { message = "Нельзя удалить сотрудника: за ним закреплены незавершённые заказы." });
        employee.IdUserNavigation.IsActive = false;
        db.EventLogs.Add(new EventLog { IdUser = employee.IdUser, EventType = "Отключение сотрудника", Description = $"Отключена учётная запись сотрудника {employee.IdUserNavigation.Login}." });
        await db.SaveChangesAsync();
        return NoContent();
    }
}
