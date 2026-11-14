using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class ProveedorRepository : Repository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(GymEliteDbContext context) : base(context) { }

    public async Task<List<Proveedor>> BuscarAsync(string? texto)
    {
        var consulta = _dbSet.AsNoTracking().Include(p => p.Compras).AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(p =>
                p.Nombre.Contains(t) ||
                p.Telefono.Contains(t) ||
                (p.Correo != null && p.Correo.Contains(t)));
        }

        return await consulta.OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<Proveedor?> GetConComprasAsync(int id) =>
        await _dbSet.AsNoTracking()
            .Include(p => p.Compras).ThenInclude(c => c.Detalles)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
    {
        // La intercalación por defecto de SQL Server no distingue mayúsculas,
        // así que "Nutri Max" y "nutri max" se consideran el mismo nombre.
        var n = nombre.Trim();
        return await _dbSet.AnyAsync(p => p.Nombre == n && (excluirId == null || p.Id != excluirId));
    }

    public async Task<bool> TieneComprasAsync(int id) =>
        await _context.Compras.AnyAsync(c => c.ProveedorId == id);
}
