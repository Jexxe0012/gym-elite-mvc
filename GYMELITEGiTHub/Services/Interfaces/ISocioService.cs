using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface ISocioService
{
    Task<List<SocioDto>> GetAllAsync(string? busqueda = null);
    Task<SocioDto?> GetByIdAsync(int id);
    Task<SocioDto> CreateAsync(SocioCreateViewModel modelo);
    Task<bool> UpdateAsync(SocioEditViewModel modelo);
    /// <summary>Desactiva la ficha; no se borra para conservar el historial.</summary>
    Task<bool> DesactivarAsync(int id);
}
