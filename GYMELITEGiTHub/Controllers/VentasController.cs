using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class VentasController : Controller
{
    private readonly IVentaService _ventas;
    public VentasController(IVentaService ventas) => _ventas = ventas;

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta) => View(new VentaListaViewModel
    {
        Desde = desde, Hasta = hasta, Ventas = await _ventas.GetAllAsync(desde, hasta)
    });

    [HttpGet]
    public async Task<IActionResult> Nueva() => View(await _ventas.PrepararNuevaAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(VentaNuevaViewModel modelo)
    {
        if (!ModelState.IsValid) return View(await ReconstruirAsync(modelo));
        try
        {
            var venta = await _ventas.RegistrarAsync(modelo);
            TempData["Exito"] = $"Venta {venta.Numero} registrada correctamente.";
            return RedirectToAction(nameof(Comprobante), new { id = venta.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await ReconstruirAsync(modelo));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Comprobante(int id)
    {
        var venta = await _ventas.GetByIdAsync(id);
        return venta is null ? NotFound() : View(venta);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            if (!await _ventas.AnularAsync(id)) return NotFound();
            TempData["Exito"] = "Venta anulada y existencias devueltas correctamente.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Comprobante), new { id });
    }

    private async Task<VentaNuevaViewModel> ReconstruirAsync(VentaNuevaViewModel enviado)
    {
        var actualizado = await _ventas.PrepararNuevaAsync();
        foreach (var linea in actualizado.Lineas)
            linea.Cantidad = enviado.Lineas.FirstOrDefault(x => x.ProductoId == linea.ProductoId)?.Cantidad ?? 0;
        return actualizado;
    }
}
