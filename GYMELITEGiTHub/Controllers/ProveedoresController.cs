using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class ProveedoresController : Controller
{
    private readonly IProveedorService _proveedores;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(IProveedorService proveedores, ILogger<ProveedoresController> logger)
    {
        _proveedores = proveedores;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var modelo = new ProveedorListaViewModel { Busqueda = busqueda };
        try
        {
            modelo.Proveedores = await _proveedores.GetAllAsync(busqueda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo consultar la lista de proveedores.");
            ModelState.AddModelError(string.Empty, "No se pudieron consultar los proveedores. Intente nuevamente.");
        }
        return View(modelo);
    }

    [HttpGet]
    public IActionResult Crear() => View(new ProveedorCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProveedorCreateViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            var proveedor = await _proveedores.CreateAsync(modelo);
            TempData["Exito"] = $"Proveedor {proveedor.Nombre} registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo registrar el proveedor {Nombre}.", modelo.Nombre);
            ModelState.AddModelError(string.Empty, "No se pudo guardar el proveedor. Intente nuevamente.");
        }

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var proveedor = await _proveedores.GetByIdAsync(id);
        if (proveedor is null) return NotFound();

        return View(new ProveedorEditViewModel
        {
            Id = proveedor.Id,
            Nombre = proveedor.Nombre,
            Telefono = proveedor.Telefono,
            Correo = proveedor.Correo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ProveedorEditViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            if (!await _proveedores.UpdateAsync(modelo)) return NotFound();
            TempData["Exito"] = "Proveedor actualizado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = modelo.Id });
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo actualizar el proveedor {ProveedorId}.", modelo.Id);
            ModelState.AddModelError(string.Empty, "No se pudieron guardar los cambios. Intente nuevamente.");
        }

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var proveedor = await _proveedores.GetByIdAsync(id);
        return proveedor is null ? NotFound() : View(proveedor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            if (!await _proveedores.EliminarAsync(id)) return NotFound();
            TempData["Exito"] = "Proveedor eliminado.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex) when (ex.InnerException is null)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo eliminar el proveedor {ProveedorId}.", id);
            TempData["Error"] = "No se pudo eliminar el proveedor. Intente nuevamente.";
        }

        return RedirectToAction(nameof(Detalle), new { id });
    }
}
