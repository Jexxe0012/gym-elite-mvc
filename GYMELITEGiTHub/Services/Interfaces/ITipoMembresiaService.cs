using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface ITipoMembresiaService
{
    Task<List<TipoMembresiaDto>> GetAllAsync(bool soloActivos = false);
    Task<TipoMembresiaDto?> GetByIdAsync(int id);
    Task<TipoMembresiaDto> CreateAsync(TipoMembresiaFormViewModel modelo);
    Task<bool> UpdateAsync(TipoMembresiaFormViewModel modelo);
    /// <summary>Activa o desactiva el plan. Devuelve null si no existe.</summary>
    Task<TipoMembresiaDto?> CambiarEstadoAsync(int id);
}
