using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IVentaService
{
    Task<List<VentaDto>> GetAllAsync(DateTime? desde, DateTime? hasta);
    Task<VentaNuevaViewModel> PrepararNuevaAsync();
    Task<VentaDto> RegistrarAsync(VentaNuevaViewModel modelo);
    Task<VentaDto?> GetByIdAsync(int id);
    Task<bool> AnularAsync(int id);
}
