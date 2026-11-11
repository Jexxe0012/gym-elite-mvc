using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class SocioRepository : Repository<Socio>, ISocioRepository
{
    public SocioRepository(GymEliteDbContext context) : base(context) { }

    public async Task<List<Socio>> BuscarAsync(string? texto)
    {
        var consulta = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(s =>
                s.NombreCompleto.Contains(t) ||
                s.Codigo.Contains(t) ||
                s.Telefono.Contains(t));
        }

        return await consulta.OrderBy(s => s.NombreCompleto).ToListAsync();
    }

    public async Task<Socio?> GetConMembresiasAsync(int id) =>
        await _dbSet
            .Include(s => s.Membresias).ThenInclude(m => m.TipoMembresia)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<string> GenerarSiguienteCodigoAsync()
    {
        var ultimoId = await _dbSet.MaxAsync(s => (int?)s.Id) ?? 0;
        return $"S-{ultimoId + 1:000}";
    }
}
