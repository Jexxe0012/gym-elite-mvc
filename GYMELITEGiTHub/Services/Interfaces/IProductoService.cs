using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IProductoService
{
    Task<ProductoListaViewModel> GetListaAsync(string? busqueda, string? categoria);
    Task<ProductoDto?> GetByIdAsync(int id);
    Task<ProductoDto> CreateAsync(ProductoCreateViewModel modelo);
    Task<bool> UpdateAsync(ProductoEditViewModel modelo);
    Task<bool> AjustarAsync(ProductoAjusteViewModel modelo);
    Task<bool> DesactivarAsync(int id);
}
