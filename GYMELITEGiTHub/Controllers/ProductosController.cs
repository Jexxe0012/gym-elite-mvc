using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class ProductosController : Controller
{
    private readonly IProductoService _productos;
    public ProductosController(IProductoService productos) => _productos = productos;

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda, string? categoria) =>
        View(await _productos.GetListaAsync(busqueda, categoria));

    [HttpGet]
    public IActionResult Crear() => View(new ProductoCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoCreateViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        try
        {
            var producto = await _productos.CreateAsync(modelo);
            TempData["Exito"] = $"Producto {producto.Codigo} registrado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = producto.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var p = await _productos.GetByIdAsync(id);
        return p is null ? NotFound() : View(new ProductoEditViewModel
        {
            Id = p.Id, Codigo = p.Codigo, Nombre = p.Nombre, Categoria = p.Categoria,
            PrecioVenta = p.PrecioVenta, StockMinimo = p.StockMinimo, Activo = p.Activo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ProductoEditViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        try
        {
            if (!await _productos.UpdateAsync(modelo)) return NotFound();
            TempData["Exito"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = modelo.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var producto = await _productos.GetByIdAsync(id);
        return producto is null ? NotFound() : View(producto);
    }

    [HttpGet]
    public async Task<IActionResult> Ajustar(int id)
    {
        var p = await _productos.GetByIdAsync(id);
        return p is null ? NotFound() : View(new ProductoAjusteViewModel
        {
            ProductoId = p.Id, Producto = $"{p.Codigo} - {p.Nombre}", ExistenciasActuales = p.Existencias
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(ProductoAjusteViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        try
        {
            if (!await _productos.AjustarAsync(modelo)) return NotFound();
            TempData["Exito"] = "Ajuste de inventario registrado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = modelo.ProductoId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        if (!await _productos.DesactivarAsync(id)) return NotFound();
        TempData["Exito"] = "Producto desactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
