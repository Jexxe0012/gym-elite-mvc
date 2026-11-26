using Microsoft.AspNetCore.Identity;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerarTokenAsync(IdentityUser usuario);
}