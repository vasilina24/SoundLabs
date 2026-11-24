using Microsoft.EntityFrameworkCore;
using SoundLabs.Models;

namespace SoundLabs.Infrastructure;

/// <summary>
/// Только для разработки: в скрипте БД у тестовых учёток стоит заглушка вместо хэша.
/// При старте заменяем её на BCrypt-хэш пароля "Pass123!".
/// </summary>
public static class DevSeeder
{
    public static async Task FixPlaceholderPasswordsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SoundLabsContext>();

        var users = await db.Users.Where(u => u.Password == "REPLACE_WITH_PASSWORD_HASH").ToListAsync();
        if (users.Count == 0) return;

        var hash = BCrypt.Net.BCrypt.HashPassword("Pass123!");
        foreach (var u in users) u.Password = hash;
        await db.SaveChangesAsync();
    }
}
