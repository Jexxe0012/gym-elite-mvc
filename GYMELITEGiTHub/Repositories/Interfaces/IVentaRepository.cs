using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

public interface IVentaRepository : IRepository<Venta>
{
    Task<List<Venta>> BuscarAsync(DateTime? desde, DateTime? hasta);
    Task<Venta?> GetConDetallesAsync(int id);
}
