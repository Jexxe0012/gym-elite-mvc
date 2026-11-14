using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<List<Producto>> BuscarAsync(string? texto, string? categoria);
    Task<Producto?> GetConMovimientosAsync(int id);
    Task<List<Producto>> GetActivosConExistenciasAsync();
    Task<bool> ExisteCodigoAsync(string codigo, int? excluirId = null);
}
