using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Password = string.Empty;
            return View(modelo);
        }

        var usuario = await _userManager.FindByEmailAsync(modelo.Email)
                      ?? await _userManager.FindByNameAsync(modelo.Email);

        if (usuario is not null)
        {
            var resultado = await _signInManager.PasswordSignInAsync(
                usuario, modelo.Password, modelo.RememberMe, lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                _logger.LogInformation("Inicio de sesión correcto del usuario {UserId}.", usuario.Id);

                if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
                    return LocalRedirect(modelo.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (resultado.IsLockedOut)
            {
                _logger.LogWarning("Cuenta bloqueada por intentos fallidos: usuario {UserId}.", usuario.Id);
                ModelState.AddModelError(string.Empty,
                    "La cuenta está bloqueada temporalmente por demasiados intentos. Intente más tarde.");
                modelo.Password = string.Empty;
                return View(modelo);
            }
        }

        _logger.LogWarning("Intento de inicio de sesión fallido.");
        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        modelo.Password = string.Empty;
        return View(modelo);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
