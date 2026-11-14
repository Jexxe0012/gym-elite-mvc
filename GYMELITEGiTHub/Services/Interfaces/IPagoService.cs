using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IPagoService
{
    Task<PagoListaViewModel> ObtenerListadoAsync(PagoListaViewModel filtros);
    Task<PagoDto?> GetByIdAsync(int id);
    Task<PagoRegistrarViewModel> PrepararRegistroAsync(int? membresiaId = null);
    Task CargarOpcionesAsync(PagoRegistrarViewModel modelo);
    Task<PagoDto> RegistrarAsync(PagoRegistrarViewModel modelo);
    Task<bool> AnularAsync(int id);
}
