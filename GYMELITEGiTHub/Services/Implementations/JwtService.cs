
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GYMELITEGiTHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace GYMELITEGiTHub.Services.Implementations;

public class JwtService : IJwtService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public JwtService(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> GenerarTokenAsync(IdentityUser usuario)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Falta Jwt:SecretKey.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Falta Jwt:Issuer.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Falta Jwt:Audience.");

        var expirationMinutes = _configuration.GetValue<int>(
            "Jwt:ExpirationMinutes");

        if (expirationMinutes <= 0)
            throw new InvalidOperationException(
                "La expiración JWT debe ser mayor que cero.");

        var keyBytes = Convert.FromBase64String(secretKey);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                "La clave JWT debe contener al menos 32 bytes.");

        // Obtener los roles asignados al usuario en Identity
        var roles = await _userManager.GetRolesAsync(usuario);

        // Construir las declaraciones (claims) del token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(ClaimTypes.Name, usuario.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(usuario.Email))
        {
            claims.Add(new Claim(
                ClaimTypes.Email,
                usuario.Email));
        }

        // Agregar los roles para que funcionen las políticas de autorización
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
