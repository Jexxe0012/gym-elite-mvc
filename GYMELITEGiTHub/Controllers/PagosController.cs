
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class PagosController : Controller
{
    private readonly IPagoService _pagos;
    private readonly ILogger<PagosController> _logger;

    public PagosController(IPagoService pagos, ILogger<PagosController> logger)
    {
        _pagos = pagos;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery, Bind("Busqueda,Estado,FechaDesde,FechaHasta")] PagoListaViewModel filtros)
    {
        if (!ModelState.IsValid) return View(filtros);

        try
        {
            return View(await _pagos.ObtenerListadoAsync(filtros));
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo consultar el historial de pagos.");
            ModelState.AddModelError(string.Empty,
                "No se pudieron consultar los pagos. Intente nuevamente en unos momentos.");
        }

        return View(filtros);
    }

    [HttpGet]
    public async Task<IActionResult> Registrar(int? membresiaId)
    {
        try
        {
            return View(await _pagos.PrepararRegistroAsync(membresiaId));
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo preparar el registro de un pago.");
            TempData["Error"] = "No se pudo cargar el formulario de cobro. Intente nuevamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar([Bind("MembresiaId")] PagoRegistrarViewModel modelo)
    {
        if (!ModelState.IsValid) return await MostrarFormularioAsync(modelo);

        try
        {
            var pago = await _pagos.RegistrarAsync(modelo);
            TempData["Exito"] = $"Pago registrado: recibo {pago.NumeroRecibo} por {pago.MontoTexto}.";
            return RedirectToAction(nameof(Recibo), new { id = pago.Id });
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el pago de la membresía {MembresiaId}.", modelo.MembresiaId);
            ModelState.AddModelError(string.Empty,
                "No se pudo confirmar el cobro. Revise el historial antes de intentar nuevamente.");
        }

        return await MostrarFormularioAsync(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Recibo(int id)
    {
        if (id <= 0) return NotFound();

        try
        {
            var pago = await _pagos.GetByIdAsync(id);
            return pago is null ? NotFound() : View(pago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo consultar el recibo del pago {PagoId}.", id);
            TempData["Error"] = "No se pudo consultar el recibo. Intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        if (id <= 0) return NotFound();

        try
        {
            if (!await _pagos.AnularAsync(id)) return NotFound();
            TempData["Exito"] = "Pago anulado. El recibo se conserva en el historial y ya no acredita cobertura.";
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo anular el pago {PagoId}.", id);
            TempData["Error"] = "No se pudo confirmar la anulación. Revise el recibo antes de intentar nuevamente.";
        }

        return RedirectToAction(nameof(Recibo), new { id });
    }

    private async Task<IActionResult> MostrarFormularioAsync(PagoRegistrarViewModel modelo)
    {
        try
        {
            await _pagos.CargarOpcionesAsync(modelo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron cargar las opciones para registrar un pago.");
            ModelState.AddModelError(string.Empty,
                "No se pudieron cargar las membresías pendientes. Intente nuevamente.");
        }

        return View(nameof(Registrar), modelo);
    }
}
