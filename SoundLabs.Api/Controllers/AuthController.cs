using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SoundLabs.Dtos;
using SoundLabs.Infrastructure;
using SoundLabs.Models;
using SoundLabs.Services;
namespace SoundLabs.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController(SoundLabsContext db, JwtService jwt) : ControllerBase
{
    /// <summary>Вход по логину и паролю. Возвращает JWT и роль пользователя.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var user = await db.Users
        .Include(u => u.IdRoleNavigation)
        .FirstOrDefaultAsync(u => u.Login == dto.Login);
        if (user is null)
        {
            Console.WriteLine($"Пользователь не найден: [{dto.Login}]");
            return Unauthorized(new { message = "Пользователь не найден" });
        }
        if (!user.IsActive)
            return Unauthorized(new { message = "Учётная запись отключена. Обратитесь к администратору." });

        if (!CheckPassword(dto.Password, user.Password))
        {
            Console.WriteLine($"Пароль не подошёл для пользователя: {user.Login}");
            return Unauthorized(new { message = "Пароль не подходит" });
        }
        user.LastLogin = DateTime.Now; // timestamp without time zone
        db.EventLogs.Add(new EventLog
        {
            EventType = "Вход в систему",
            Description = $"Пользователь {user.Login} вошёл в систему",
            IdUser = user.IdUser
        });
        await db.SaveChangesAsync();
        var role = user.IdRoleNavigation.RoleName;
        var (token, expires) = jwt.Create(user, role);
        return new AuthResponse(token, role, user.Login, expires);
    }
    /// <summary>Получение профиля текущего пользователя.</summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = User.GetUserId();
        var user = await db.Users.Include(u => u.IdRoleNavigation)
            .FirstOrDefaultAsync(u => u.IdUser == userId);
        if (user is null) return NotFound(new { message = "Пользователь не найден" });

        var role = user.IdRoleNavigation.RoleName;
        if (role == Roles.Client)
        {
            var client = await db.Clients.FirstOrDefaultAsync(c => c.IdUser == userId);
            if (client is null) return NotFound(new { message = "Карточка покупателя не найдена" });
            return new ProfileDto(user.Login, role, client.FullName, client.Phone, client.Email, null);
        }

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.IdUser == userId);
        if (employee is null) return NotFound(new { message = "Карточка сотрудника не найдена" });
        return new ProfileDto(user.Login, role, employee.FullName, employee.Phone, employee.Email, employee.JobTitle);
    }

    /// <summary>Изменение контактных данных текущего пользователя.</summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = User.GetUserId();
        var user = await db.Users.Include(u => u.IdRoleNavigation)
            .FirstOrDefaultAsync(u => u.IdUser == userId);
        if (user is null) return NotFound(new { message = "Пользователь не найден" });

        var email = dto.Email.Trim();
        var duplicateClientEmail = await db.Clients.AnyAsync(c => c.Email == email && c.IdUser != userId);
        var duplicateEmployeeEmail = await db.Employees.AnyAsync(e => e.Email == email && e.IdUser != userId);
        if (duplicateClientEmail || duplicateEmployeeEmail)
            return Conflict(new { message = "Этот адрес электронной почты уже используется" });

        if (user.IdRoleNavigation.RoleName == Roles.Client)
        {
            var client = await db.Clients.FirstOrDefaultAsync(c => c.IdUser == userId);
            if (client is null) return NotFound(new { message = "Карточка покупателя не найдена" });
            client.FullName = dto.FullName.Trim();
            client.Phone = dto.Phone.Trim();
            client.Email = email;
        }
        else
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.IdUser == userId);
            if (employee is null) return NotFound(new { message = "Карточка сотрудника не найдена" });
            employee.FullName = dto.FullName.Trim();
            employee.Phone = dto.Phone.Trim();
            employee.Email = email;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Самостоятельная регистрация покупателя.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto dto)

    {
        if (await db.Users.AnyAsync(u => u.Login == dto.Login))
            return Conflict(new { message = "Логин уже занят" });
        if (await db.Clients.AnyAsync(c => c.Email == dto.Email))
            return Conflict(new { message = "Клиент с таким email уже зарегистрирован" });
        var roleId = await db.Roles.Where(r => r.RoleName == Roles.Client)
        .Select(r => r.IdRole).SingleAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = new User
        {
            Login = dto.Login,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IdRole = roleId
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.Clients.Add(new Client
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            IdUser = user.IdUser
        });
        db.EventLogs.Add(new EventLog
        {
            EventType = "Регистрация",
            Description = $"Зарегистрирован покупатель {dto.Login}",
            IdUser = user.IdUser
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        var (token, expires) = jwt.Create(user, Roles.Client);
        return Created("", new AuthResponse(token, Roles.Client, user.Login, expires));
    }
    private static bool CheckPassword(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; } // в БД лежит не BCrypt-хэш
    }
}
