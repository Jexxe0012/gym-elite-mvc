using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class PagoRepository : Repository<Pago>, IPagoRepository
{
    public PagoRepository(GymEliteDbContext context) : base(context) { }

    public async Task<bool> ExistePagoValidoAsync(int membresiaId) =>
        await _dbSet.AnyAsync(p =>
            p.MembresiaId == membresiaId && p.Estado == EstadosNegocio.PagoRegistrado);

    public async Task<string> GenerarSiguienteNumeroReciboAsync()
    {
        var ultimoId = await _dbSet.MaxAsync(p => (int?)p.Id) ?? 0;
        return $"P-{ultimoId + 1:0000}";
    }
}
