using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class VentaRepository : Repository<Venta>, IVentaRepository
{
    public VentaRepository(GymEliteDbContext context) : base(context) { }

    public async Task<List<Venta>> BuscarAsync(DateTime? desde, DateTime? hasta)
    {
        var consulta = _dbSet.AsNoTracking().AsQueryable();
        if (desde.HasValue) consulta = consulta.Where(v => v.Fecha >= desde.Value.Date);
        if (hasta.HasValue) consulta = consulta.Where(v => v.Fecha < hasta.Value.Date.AddDays(1));
        return await consulta.OrderByDescending(v => v.Fecha).ToListAsync();
    }

    public async Task<Venta?> GetConDetallesAsync(int id) =>
        await _dbSet.Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
}
