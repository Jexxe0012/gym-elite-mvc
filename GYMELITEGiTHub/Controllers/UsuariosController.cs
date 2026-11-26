using GYMELITEGiTHub.Security;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.Controllers;

[Authorize(Roles = RolesSistema.Administrador)]
public class UsuariosController : Controller
{
    private static readonly string[] RolesDisponibles =
    {
        RolesSistema.Administrador, RolesSistema.Recepcion, RolesSistema.Inventario
    };

    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(UserManager<IdentityUser> userManager, ILogger<UsuariosController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    private static bool EstaDeshabilitado(IdentityUser u) =>
        u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow;

    private static IEnumerable<SelectListItem> OpcionesRol() =>
        RolesDisponibles.Select(r => new SelectListItem(r == RolesSistema.Recepcion ? "Recepción" : r, r));

    private async Task<bool> EsUltimoAdminHabilitadoAsync(IdentityUser usuario)
    {
        if (!await _userManager.IsInRoleAsync(usuario, RolesSistema.Administrador))
            return false;

        var admins = await _userManager.GetUsersInRoleAsync(RolesSistema.Administrador);
        return !admins.Any(a => a.Id != usuario.Id && !EstaDeshabilitado(a));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var actualId = _userManager.GetUserId(User);
        var modelo = new UsuarioListaViewModel();

        foreach (var u in _userManager.Users.OrderBy(u => u.Email).ToList())
        {
            var roles = await _userManager.GetRolesAsync(u);
            modelo.Usuarios.Add(new UsuarioItemViewModel
            {
                Id = u.Id,
                Correo = u.Email ?? u.UserName ?? string.Empty,
                Rol = roles.Count > 0 ? string.Join(", ", roles) : "Sin rol",
                Habilitado = !EstaDeshabilitado(u),
                EsUsuarioActual = u.Id == actualId
            });
        }

        return View(modelo);
    }

    [HttpGet]
    public IActionResult Crear() => View(new UsuarioCrearViewModel { Roles = OpcionesRol() });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
    {
        modelo.Roles = OpcionesRol();

        if (!RolesDisponibles.Contains(modelo.Rol))
            ModelState.AddModelError(nameof(modelo.Rol), "Seleccione un rol válido.");

        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = new IdentityUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            EmailConfirmed = true
        };

        var creado = await _userManager.CreateAsync(usuario, modelo.Password);
        if (!creado.Succeeded)
        {
            foreach (var e in creado.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(modelo);
        }

        var conRol = await _userManager.AddToRoleAsync(usuario, modelo.Rol);
        if (!conRol.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);
            ModelState.AddModelError(string.Empty, "No se pudo asignar el rol. El usuario no fue creado.");
            return View(modelo);
        }

        _logger.LogInformation("Usuario {UserId} creado con rol {Rol} por {Admin}.",
            usuario.Id, modelo.Rol, _userManager.GetUserId(User));
        TempData["Exito"] = $"Usuario {modelo.Correo} creado con el rol {modelo.Rol}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(usuario);
        return View(new UsuarioEditarViewModel
        {
            Id = usuario.Id,
            Correo = usuario.Email ?? usuario.UserName ?? string.Empty,
            Rol = roles.FirstOrDefault() ?? string.Empty,
            Roles = OpcionesRol()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(UsuarioEditarViewModel modelo)
    {
        modelo.Roles = OpcionesRol();

        var usuario = await _userManager.FindByIdAsync(modelo.Id);
        if (usuario is null) return NotFound();
        modelo.Correo = usuario.Email ?? usuario.UserName ?? string.Empty;

        if (!RolesDisponibles.Contains(modelo.Rol))
            ModelState.AddModelError(nameof(modelo.Rol), "Seleccione un rol válido.");

        if (modelo.Rol != RolesSistema.Administrador)
        {
            if (usuario.Id == _userManager.GetUserId(User))
                ModelState.AddModelError(string.Empty, "No puede quitarse su propio rol de administrador.");
            else if (await EsUltimoAdminHabilitadoAsync(usuario))
                ModelState.AddModelError(string.Empty, "Debe existir al menos un administrador habilitado.");
        }

        if (!ModelState.IsValid)
            return View(modelo);

        var actuales = await _userManager.GetRolesAsync(usuario);
        if (!actuales.Contains(modelo.Rol))
        {
            var agregado = await _userManager.AddToRoleAsync(usuario, modelo.Rol);
            if (!agregado.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "No se pudo asignar el rol.");
                return View(modelo);
            }
        }

        var sobrantes = actuales.Where(r => r != modelo.Rol).ToList();
        if (sobrantes.Count > 0)
            await _userManager.RemoveFromRolesAsync(usuario, sobrantes);

        await _userManager.UpdateSecurityStampAsync(usuario);

        _logger.LogInformation("Rol de {UserId} cambiado a {Rol} por {Admin}.",
            usuario.Id, modelo.Rol, _userManager.GetUserId(User));
        TempData["Exito"] = $"Rol de {modelo.Correo} actualizado a {modelo.Rol}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null) return NotFound();

        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "No puede deshabilitar su propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        if (EstaDeshabilitado(usuario))
        {
            await _userManager.SetLockoutEndDateAsync(usuario, null);
            await _userManager.ResetAccessFailedCountAsync(usuario);
            TempData["Exito"] = $"Usuario {usuario.Email} habilitado.";
            _logger.LogInformation("Usuario {UserId} habilitado por {Admin}.", usuario.Id, _userManager.GetUserId(User));
        }
        else
        {
            if (await EsUltimoAdminHabilitadoAsync(usuario))
            {
                TempData["Error"] = "Debe existir al menos un administrador habilitado.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEnabledAsync(usuario, true);
            await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
            await _userManager.UpdateSecurityStampAsync(usuario);
            TempData["Exito"] = $"Usuario {usuario.Email} deshabilitado.";
            _logger.LogInformation("Usuario {UserId} deshabilitado por {Admin}.", usuario.Id, _userManager.GetUserId(User));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RestablecerContrasena(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null) return NotFound();

        return View(new UsuarioPasswordViewModel
        {
            Id = usuario.Id,
            Correo = usuario.Email ?? usuario.UserName ?? string.Empty
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasena(UsuarioPasswordViewModel modelo)
    {
        var usuario = await _userManager.FindByIdAsync(modelo.Id);
        if (usuario is null) return NotFound();
        modelo.Correo = usuario.Email ?? usuario.UserName ?? string.Empty;

        if (!ModelState.IsValid)
            return View(modelo);

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, modelo.NuevaPassword);

        if (!resultado.Succeeded)
        {
            foreach (var e in resultado.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(modelo);
        }

        _logger.LogInformation("Contraseña de {UserId} restablecida por {Admin}.",
            usuario.Id, _userManager.GetUserId(User));
        TempData["Exito"] = $"Contraseña de {modelo.Correo} restablecida.";
        return RedirectToAction(nameof(Index));
    }
}
