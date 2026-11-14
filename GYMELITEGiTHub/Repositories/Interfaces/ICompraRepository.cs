using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>Consultas de compras y sus líneas. No confirma cambios: eso lo hace IUnitOfWork.</summary>
public interface ICompraRepository : IRepository<Compra>
{
    Task<List<Compra>> BuscarAsync(int? proveedorId, string? estado, DateOnly? desde, DateOnly? hasta);

    /// <summary>Compra con proveedor y líneas (cada una con su producto), sin seguimiento. Para mostrar.</summary>
    Task<Compra?> GetConDetalleAsync(int id);

    /// <summary>Compra con sus líneas, con seguimiento. Para anularla.</summary>
    Task<Compra?> GetParaAnularAsync(int id);

    /// <summary>Compras registradas (no anuladas) y monto comprado entre dos fechas, inclusive.</summary>
    Task<ResumenComprasDto> ObtenerResumenAsync(DateOnly desde, DateOnly hasta);
}
