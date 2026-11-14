using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IProveedorService
{
    Task<List<ProveedorDto>> GetAllAsync(string? busqueda = null);
    Task<ProveedorDto?> GetByIdAsync(int id);
    Task<ProveedorDto> CreateAsync(ProveedorCreateViewModel modelo);
    Task<bool> UpdateAsync(ProveedorEditViewModel modelo);

    /// <summary>
    /// Borrado real. Solo se permite si el proveedor no tiene compras;
    /// si las tiene, lanza InvalidOperationException y no borra nada.
    /// </summary>
    Task<bool> EliminarAsync(int id);
}
