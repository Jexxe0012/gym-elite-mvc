using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class AccesosController : Controller
{
    private readonly IAccesoService _accesos;
    private readonly ILogger<AccesosController> _logger;

    public AccesosController(IAccesoService accesos, ILogger<AccesosController> logger)
    {
        _accesos = accesos;
        _logger = logger;
    }

    /// <summary>Pantalla de recepción. Con ?busqueda= verifica al socio (QR, código, nombre o teléfono).</summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var modelo = new AccesoVerificarViewModel { Busqueda = busqueda?.Trim() };

        try
        {
            await _accesos.PrepararAsync(modelo);
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo verificar el acceso con la búsqueda '{Busqueda}'.", busqueda);
            ModelState.AddModelError(string.Empty,
                "No se pudo completar la verificación. Intente nuevamente en unos momentos.");
        }

        return View(modelo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int socioId)
    {
        if (socioId <= 0)
        {
            TempData["Error"] = "Seleccione un socio antes de registrar el ingreso.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var acceso = await _accesos.RegistrarIngresoAsync(socioId);
            TempData["Exito"] =
                $"Ingreso registrado: {acceso.SocioCodigo} · {acceso.SocioNombre} a las {acceso.FechaHoraIngreso:HH:mm}.";
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el ingreso del socio {SocioId}.", socioId);
            TempData["Error"] = "No se pudo confirmar el ingreso. Revise los últimos accesos antes de intentar nuevamente.";
        }

        // Vuelve a la pantalla limpia, lista para el siguiente socio.
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Historial(
        [FromQuery, Bind("Busqueda,FechaDesde,FechaHasta")] AccesoHistorialViewModel filtros)
    {
        if (!ModelState.IsValid) return View(filtros);

        try
        {
            return View(await _accesos.ObtenerHistorialAsync(filtros));
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo consultar el historial de accesos.");
            ModelState.AddModelError(string.Empty,
                "No se pudo consultar el historial. Intente nuevamente en unos momentos.");
        }

        return View(filtros);
    }
}
