using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class SociosController : Controller
{
    private readonly ISocioService _socioService;

    public SociosController(ISocioService socioService) => _socioService = socioService;

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda)
    {
        var modelo = new SocioListaViewModel
        {
            Busqueda = busqueda,
            Socios = await _socioService.GetAllAsync(busqueda)
        };
        return View(modelo);
    }

    [HttpGet]
    public IActionResult Crear() => View(new SocioCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(SocioCreateViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var socio = await _socioService.CreateAsync(modelo);
        TempData["Exito"] = $"Socio {socio.Codigo} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var socio = await _socioService.GetByIdAsync(id);
        if (socio is null) return NotFound();

        return View(new SocioEditViewModel
        {
            Id = socio.Id,
            Codigo = socio.Codigo,
            NombreCompleto = socio.NombreCompleto,
            Telefono = socio.Telefono,
            Correo = socio.Correo,
            FechaNacimiento = socio.FechaNacimiento,
            EstaHabilitado = socio.EstaHabilitado
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(SocioEditViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        if (!await _socioService.UpdateAsync(modelo)) return NotFound();

        TempData["Exito"] = "Socio actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var socio = await _socioService.GetByIdAsync(id);
        return socio is null ? NotFound() : View(socio);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        if (!await _socioService.DesactivarAsync(id)) return NotFound();

        TempData["Exito"] = "La ficha del socio fue desactivada.";
        return RedirectToAction(nameof(Index));
    }
}
