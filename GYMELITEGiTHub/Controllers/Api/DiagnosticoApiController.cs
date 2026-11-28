using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

// Prueba controlada del manejo de errores de la API (Etapa 4, apartado 6).
// Solo Administrador y solo si Diagnostico:HabilitarPruebaError = true; si no, responde 404.
[Route("api/diagnostico")]
[Authorize(Policy = ApiConfiguracion.Administracion)]
public sealed class DiagnosticoApiController(
    IConfiguration configuracion, ILogger<DiagnosticoApiController> logger) : ApiControllerBase
{
    [HttpGet("error")]
    public IActionResult GenerarError()
    {
        if (!configuracion.GetValue<bool>("Diagnostico:HabilitarPruebaError"))
            return NotFound();

        logger.LogWarning("Se solicitó la prueba controlada de error desde la API.");
        throw new InvalidOperationException("Excepción de prueba generada desde /api/diagnostico/error.");
    }
}
