using GYMELITEGiTHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Controllers.Api;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensaje = "Credenciales incorrectas."
            });
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(
            usuario,
            request.Password,
            lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            return Unauthorized(new
            {
                mensaje = "Credenciales incorrectas."
            });
        }

        var token = await _jwtService.GenerarTokenAsync(usuario);

        var minutos = _configuration.GetValue<int>(
            "Jwt:ExpirationMinutes");

        return Ok(new
        {
            accessToken = token,
            tokenType = "Bearer",
            expiresAt = DateTime.UtcNow.AddMinutes(minutos)
        });
    }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}