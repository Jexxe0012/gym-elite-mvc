using System.Text.Json;
using GYMELITEGiTHub.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GYMELITEGiTHub.Infraestructura;

// Monitoreo de disponibilidad (Etapa 4, apartado 6.6).
// GET /health/vivo  -> la aplicación responde (no toca la base).
// GET /health       -> aplicación y conexión con la base de datos.
// Responden 200 si todo está bien y 503 si algo falla. No exponen mensajes de error ni datos de conexión.
public static class Monitoreo
{
    public static void AgregarMonitoreo(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<GymEliteDbContext>("base_de_datos", tags: new[] { "bd" });
    }

    public static void MapearMonitoreo(this WebApplication app)
    {
        app.MapHealthChecks("/health/vivo", new()
        {
            Predicate = _ => false,
            ResponseWriter = EscribirRespuesta
        });

        app.MapHealthChecks("/health", new()
        {
            ResponseWriter = EscribirRespuesta
        });
    }

    private static Task EscribirRespuesta(HttpContext http, HealthReport reporte)
    {
        http.Response.ContentType = "application/json; charset=utf-8";

        var cuerpo = new
        {
            estado = reporte.Status.ToString(),
            duracionMs = Math.Round(reporte.TotalDuration.TotalMilliseconds, 1),
            fechaUtc = DateTime.UtcNow,
            comprobaciones = reporte.Entries.Select(e => new
            {
                nombre = e.Key,
                estado = e.Value.Status.ToString(),
                duracionMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1)
            })
        };

        return http.Response.WriteAsync(JsonSerializer.Serialize(cuerpo,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
