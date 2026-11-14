using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface ICompraService
{
    Task<CompraListaViewModel> ObtenerListadoAsync(CompraListaViewModel filtros);
    Task<CompraDto?> GetByIdAsync(int id);

    /// <summary>Formulario vacío con una línea lista para llenar.</summary>
    Task<CompraNuevaViewModel> PrepararNuevaAsync(int? proveedorId = null);

    /// <summary>Carga los desplegables de proveedores y productos activos.</summary>
    Task CargarOpcionesAsync(CompraNuevaViewModel modelo);

    /// <summary>Guarda la compra, sus líneas y las entradas de inventario en una sola transacción.</summary>
    Task<CompraDto> RegistrarAsync(CompraNuevaViewModel modelo);

    /// <summary>Marca la compra como anulada y revierte sus entradas de inventario.</summary>
    Task<bool> AnularAsync(int id);
}
