using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoundLabs.Infrastructure;
using SoundLabs.Models;
using System.Diagnostics;

namespace SoundLabs.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController(SoundLabsContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings() => Ok(await db.SystemSettings.AsNoTracking()
        .OrderBy(x => x.ParamName).Select(x => new { id = x.IdSetting, name = x.ParamName, value = x.ParamValue, description = x.Description }).ToListAsync());

    public record SettingUpdate(string Value);
    public record SettingCreate(string Name, string Value, string? Description);
    [HttpPost("settings")]
    public async Task<IActionResult> CreateSetting(SettingCreate dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 50 || dto.Value is null || dto.Value.Length > 255)
            return BadRequest(new { message = "Укажите название параметра (до 50 символов) и значение (до 255 символов)." });
        var name = dto.Name.Trim();
        if (await db.SystemSettings.AnyAsync(x => x.ParamName == name)) return Conflict(new { message = "Параметр с таким названием уже существует." });
        db.SystemSettings.Add(new SystemSetting { ParamName = name, ParamValue = dto.Value.Trim(), Description = dto.Description?.Trim() });
        Log("Создание настройки", $"Добавлен параметр системы {name}.");
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("settings/{id:int}")]
    public async Task<IActionResult> UpdateSetting(int id, SettingUpdate dto)
    {
        var setting = await db.SystemSettings.FindAsync(id);
        if (setting is null) return NotFound();
        if (dto.Value is null || dto.Value.Length > 255) return BadRequest(new { message = "Значение должно содержать не более 255 символов." });
        setting.ParamValue = dto.Value.Trim();
        Log("Изменение настройки", $"Изменён параметр системы {setting.ParamName}.");
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int take = 200) => Ok(await db.EventLogs.AsNoTracking()
        .OrderByDescending(x => x.EventDate).Take(Math.Clamp(take, 1, 1000))
        .Select(x => new { id = x.IdLog, date = x.EventDate, type = x.EventType, description = x.Description, login = x.IdUserNavigation != null ? x.IdUserNavigation.Login : "Система" }).ToListAsync());

    [HttpGet("backup")]
    public async Task<IActionResult> Backup()
    {
        var cs = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(cs)) return StatusCode(500, new { message = "Не настроена строка подключения к базе данных." });
        var builder = new NpgsqlConnectionStringBuilder(cs);
        var file = Path.Combine(Path.GetTempPath(), $"soundlabs_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");
        var args = $"--host=\"{builder.Host}\" --port={builder.Port} --username=\"{builder.Username}\" --dbname=\"{builder.Database}\" --no-owner --no-privileges --file=\"{file}\"";
        var start = new ProcessStartInfo("pg_dump", args) { RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        if (!string.IsNullOrEmpty(builder.Password)) start.Environment["PGPASSWORD"] = builder.Password;
        try
        {
            using var process = Process.Start(start);
            if (process is null) return StatusCode(500, new { message = "Не удалось запустить pg_dump. Установите PostgreSQL client tools." });
            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var error = await errorTask;
            if (process.ExitCode != 0 || !System.IO.File.Exists(file)) return StatusCode(500, new { message = "Резервная копия не создана: " + error });
            Log("Резервное копирование", "Администратор создал резервную копию базы данных.");
            await db.SaveChangesAsync();
            var bytes = await System.IO.File.ReadAllBytesAsync(file);
            System.IO.File.Delete(file);
            return File(bytes, "application/sql", $"SoundLabs_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or FileNotFoundException)
        { return StatusCode(500, new { message = "Команда pg_dump не найдена. Установите PostgreSQL client tools и добавьте pg_dump в PATH." }); }
        finally { if (System.IO.File.Exists(file)) System.IO.File.Delete(file); }
    }

    private void Log(string type, string description) => db.EventLogs.Add(new SoundLabs.Models.EventLog { IdUser = User.GetUserId(), EventType = type, Description = description });
}
