
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace GYMELITEGiTHub.Controllers;

public class ReportesController : Controller
{
    private readonly IReporteService _reportes;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IReporteService reportes, ILogger<ReportesController> logger)
    {
        _reportes = reportes;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery, Bind("FechaDesde,FechaHasta")] ReporteViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            return View(await _reportes.GenerarAsync(modelo));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo generar el reporte del período {Desde} a {Hasta}.",
                modelo.FechaDesde, modelo.FechaHasta);
            ModelState.AddModelError(string.Empty,
                "No se pudo generar el reporte. Intente nuevamente en unos momentos.");
        }

        return View(modelo);
    }
}
