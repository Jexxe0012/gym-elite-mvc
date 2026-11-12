using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class MembresiasController : Controller
{
    private readonly IMembresiaService _membresiaService;
    private readonly ITipoMembresiaService _tipoService;

    public MembresiasController(IMembresiaService membresiaService, ITipoMembresiaService tipoService)
    {
        _membresiaService = membresiaService;
        _tipoService = tipoService;
    }

    // ---------- Membresías ----------

    [HttpGet]
    public async Task<IActionResult> Index(string? busqueda, string? estado)
    {
        var modelo = new MembresiaListaViewModel
        {
            Busqueda = busqueda,
            Estado = estado,
            Membresias = await _membresiaService.GetAllAsync(busqueda, estado)
        };
        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var membresia = await _membresiaService.GetByIdAsync(id);
        return membresia is null ? NotFound() : View(membresia);
    }

    [HttpGet]
    public async Task<IActionResult> Crear() =>
        View(await _membresiaService.PrepararNuevaAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(MembresiaCreateViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await _membresiaService.CargarOpcionesAsync(modelo);
            return View(modelo);
        }

        try
        {
            var membresia = await _membresiaService.CreateAsync(modelo);

            if (membresia.TienePagoValido)
            {
                TempData["Exito"] =
                    $"Membresía registrada y cobrada: recibo {membresia.NumeroRecibo} por {membresia.MontoPagadoTexto}. " +
                    $"Vigente hasta el {membresia.FechaFin:dd/MM/yyyy}.";
            }
            else
            {
                TempData["Aviso"] =
                    "Membresía registrada como PENDIENTE DE PAGO. No dará acceso al gimnasio hasta que se cobre.";
            }

            return RedirectToAction(nameof(Detalle), new { id = membresia.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await _membresiaService.CargarOpcionesAsync(modelo);
            return View(modelo);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Renovar(int socioId)
    {
        try
        {
            var modelo = await _membresiaService.PrepararRenovacionAsync(socioId);
            if (modelo is null) return NotFound();
            return View(nameof(Crear), modelo);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            var resultado = await _membresiaService.CancelarAsync(id);
            if (resultado is null) return NotFound();

            TempData["Exito"] = resultado.ReciboAnulado is null
                ? "Membresía cancelada."
                : $"Membresía cancelada. Se anuló el recibo {resultado.ReciboAnulado}: devuelva el efectivo al cliente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detalle), new { id });
    }

    // ---------- Planes (tipos de membresía) ----------

    [HttpGet]
    public async Task<IActionResult> Planes() =>
        View(await _tipoService.GetAllAsync());

    [HttpGet]
    public IActionResult CrearPlan() => View(new TipoMembresiaFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPlan(TipoMembresiaFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            var plan = await _tipoService.CreateAsync(modelo);
            TempData["Exito"] = $"Plan \"{plan.Nombre}\" creado correctamente.";
            return RedirectToAction(nameof(Planes));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditarPlan(int id)
    {
        var plan = await _tipoService.GetByIdAsync(id);
        if (plan is null) return NotFound();

        return View(new TipoMembresiaFormViewModel
        {
            Id = plan.Id,
            Nombre = plan.Nombre,
            Modalidad = plan.Modalidad,
            DuracionDias = plan.DuracionDias,
            Precio = plan.Precio,
            Activo = plan.Activo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPlan(TipoMembresiaFormViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            if (!await _tipoService.UpdateAsync(modelo)) return NotFound();
            TempData["Exito"] = "Plan actualizado correctamente.";
            return RedirectToAction(nameof(Planes));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoPlan(int id)
    {
        var plan = await _tipoService.CambiarEstadoAsync(id);
        if (plan is null) return NotFound();

        TempData["Exito"] = plan.Activo
            ? $"Plan \"{plan.Nombre}\" activado."
            : $"Plan \"{plan.Nombre}\" desactivado.";
        return RedirectToAction(nameof(Planes));
    }
}
