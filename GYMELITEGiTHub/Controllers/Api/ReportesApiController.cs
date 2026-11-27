using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/reportes")]
[Authorize(Policy = ApiConfiguracion.Administracion)]
public sealed class ReportesApiController(IReporteService reportes) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ReporteViewModel>> Obtener([FromQuery] PeriodoApi periodo)
    {
        var filtros = new ReporteViewModel();
        filtros.FechaDesde = periodo.FechaDesde ?? filtros.FechaDesde;
        filtros.FechaHasta = periodo.FechaHasta ?? filtros.FechaHasta;
        return await reportes.GenerarAsync(filtros);
    }
}
