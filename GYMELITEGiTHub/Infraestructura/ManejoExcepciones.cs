using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Infraestructura;

// Manejo común de excepciones no controladas (Etapa 4, apartado 6).
// - Rutas /api: respuesta application/problem+json con estado 500 y traceId, sin detalle técnico.
// - Pantallas MVC: devuelve false y UseExceptionHandler("/Home/Error") muestra la página controlada.
// El detalle técnico queda en el registro (línea de la solicitud con la excepción completa).
// Los errores previstos de los controladores API los sigue resolviendo ApiExceptionFilter.
public sealed class ManejadorExcepcionesApi(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext http, Exception excepcion, CancellationToken cancelacion)
    {
        if (!http.Request.Path.StartsWithSegments("/api"))
            return false;

        http.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor.",
                Detail = "No se pudo completar la solicitud. Comunique el traceId al administrador del sistema."
            }
        });
    }
}

public static class ManejoExcepciones
{
    public static void AgregarManejoExcepciones(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ManejadorExcepcionesApi>();
    }

    public static void UsarManejoExcepciones(this WebApplication app)
    {
        // Excepciones no controladas en API y MVC, en todos los entornos.
        app.UseExceptionHandler("/Home/Error");

        // Códigos 404, 403, etc. sin cuerpo en pantallas MVC. La API conserva sus respuestas JSON.
        app.UseWhen(
            http => !http.Request.Path.StartsWithSegments("/api"),
            mvc => mvc.UseStatusCodePagesWithReExecute("/Home/Estado/{0}"));
    }
}
