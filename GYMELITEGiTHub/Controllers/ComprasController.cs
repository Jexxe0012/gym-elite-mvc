using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class ComprasController : Controller
{
    private readonly ICompraService _compras;
    private readonly ILogger<ComprasController> _logger;

    public ComprasController(ICompraService compras, ILogger<ComprasController> logger)
    {
        _compras = compras;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery, Bind("ProveedorId,Estado,FechaDesde,FechaHasta")] CompraListaViewModel filtros)
    {
        if (!ModelState.IsValid)
        {
            // Los filtros no son válidos: se muestra la pantalla sin resultados, con el mensaje.
            return await VistaListadoSinFiltrosAsync(filtros);
        }

        try
        {
            return View(await _compras.ObtenerListadoAsync(filtros));
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo consultar el listado de compras.");
            ModelState.AddModelError(string.Empty, "No se pudieron consultar las compras. Intente nuevamente.");
        }

        return View(filtros);
    }

    [HttpGet]
    public async Task<IActionResult> Nueva(int? proveedorId)
    {
        try
        {
            return View(await _compras.PrepararNuevaAsync(proveedorId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo preparar el formulario de compra.");
            TempData["Error"] = "No se pudo cargar el formulario de compra. Intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva([Bind("ProveedorId,Lineas")] CompraNuevaViewModel modelo)
    {
        if (!ModelState.IsValid) return await MostrarFormularioAsync(modelo);

        try
        {
            var compra = await _compras.RegistrarAsync(modelo);
            TempData["Exito"] = $"Compra {compra.Numero} registrada por {compra.TotalTexto}. Las existencias ya se actualizaron.";
            return RedirectToAction(nameof(Detalle), new { id = compra.Id });
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar la compra del proveedor {ProveedorId}.", modelo.ProveedorId);
            ModelState.AddModelError(string.Empty,
                "No se pudo registrar la compra. No se guardó ningún cambio; intente nuevamente.");
        }

        return await MostrarFormularioAsync(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        if (id <= 0) return NotFound();

        try
        {
            var compra = await _compras.GetByIdAsync(id);
            return compra is null ? NotFound() : View(compra);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo consultar la compra {CompraId}.", id);
            TempData["Error"] = "No se pudo consultar la compra. Intente nuevamente.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        if (id <= 0) return NotFound();

        try
        {
            if (!await _compras.AnularAsync(id)) return NotFound();
            TempData["Exito"] = "Compra anulada. Se registró la salida de inventario y la compra se conserva en el historial.";
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo anular la compra {CompraId}.", id);
            TempData["Error"] = "No se pudo anular la compra. No se guardó ningún cambio.";
        }

        return RedirectToAction(nameof(Detalle), new { id });
    }

    private async Task<IActionResult> MostrarFormularioAsync(CompraNuevaViewModel modelo)
    {
        try
        {
            await _compras.CargarOpcionesAsync(modelo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron cargar proveedores y productos para la compra.");
            ModelState.AddModelError(string.Empty, "No se pudieron cargar proveedores y productos. Intente nuevamente.");
        }

        return View(nameof(Nueva), modelo);
    }

    private async Task<IActionResult> VistaListadoSinFiltrosAsync(CompraListaViewModel filtros)
    {
        try
        {
            var opciones = await _compras.ObtenerListadoAsync(new CompraListaViewModel());
            filtros.Proveedores = opciones.Proveedores;
            filtros.ResumenMes = opciones.ResumenMes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron cargar las opciones del listado de compras.");
        }

        return View(nameof(Index), filtros);
    }
}
