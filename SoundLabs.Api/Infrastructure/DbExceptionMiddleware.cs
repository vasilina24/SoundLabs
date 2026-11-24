using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SoundLabs.Infrastructure;

/// <summary>Преобразует ошибки PostgreSQL в понятные ответы API и сохраняет подробности в журнале.</summary>
public class DbExceptionMiddleware(RequestDelegate next, ILogger<DbExceptionMiddleware> logger, IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex) when (!ctx.Response.HasStarted)
        {
            var pg = FindPostgresException(ex);
            if (pg is not null)
            {
                var (status, message) = pg.SqlState switch
                {
                    "P0001" => (400, pg.MessageText),
                    "23505" => (409, "Запись с такими данными уже существует"),
                    "23503" => (409, "Операция нарушает связи данных: отсутствует связанная запись или запись используется"),
                    "23514" => (400, "Статус или другие данные не разрешены ограничениями базы данных"),
                    "23502" => (400, "Не заполнено обязательное поле"),
                    _ => (500, "Ошибка базы данных при сохранении изменений")
                };

                logger.LogError(ex, "Ошибка PostgreSQL {SqlState}, constraint {Constraint}", pg.SqlState, pg.ConstraintName);
                ctx.Response.Clear();
                ctx.Response.StatusCode = status;
                await ctx.Response.WriteAsJsonAsync(new
                {
                    message,
                    detail = environment.IsDevelopment() ? pg.MessageText : null,
                    constraint = environment.IsDevelopment() ? pg.ConstraintName : null
                });
                return;
            }

            logger.LogError(ex, "Необработанная ошибка API для {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
            ctx.Response.Clear();
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new
            {
                message = "Не удалось выполнить операцию. Проверьте журнал API для подробностей.",
                detail = environment.IsDevelopment() ? ex.Message : null
            });
        }
    }

    private static PostgresException? FindPostgresException(Exception ex)
    {
        for (Exception? current = ex; current is not null; current = current.InnerException)
            if (current is PostgresException pg) return pg;
        return null;
    }
}
