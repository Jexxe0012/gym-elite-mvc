using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class ProductoRepository : Repository<Producto>, IProductoRepository
{
    public ProductoRepository(GymEliteDbContext context) : base(context) { }

    public async Task<List<Producto>> BuscarAsync(string? texto, string? categoria)
    {
        var consulta = _dbSet.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(t) || p.Codigo.Contains(t));
        }
        if (!string.IsNullOrWhiteSpace(categoria))
            consulta = consulta.Where(p => p.Categoria == categoria);

        return await consulta.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<Producto?> GetConMovimientosAsync(int id) =>
        await _dbSet.Include(p => p.Movimientos.OrderByDescending(m => m.FechaMovimiento))
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Producto>> GetActivosConExistenciasAsync() =>
        await _dbSet.AsNoTracking().Where(p => p.Activo && p.Existencias > 0)
            .OrderBy(p => p.Nombre).ToListAsync();

    public Task<bool> ExisteCodigoAsync(string codigo, int? excluirId = null) =>
        _dbSet.AnyAsync(p => p.Codigo == codigo && (!excluirId.HasValue || p.Id != excluirId.Value));
}
