using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class PagoRepository : Repository<Pago>, IPagoRepository
{
    public PagoRepository(GymEliteDbContext context) : base(context) { }

    private IQueryable<Pago> ConDetalle() =>
        _dbSet.AsNoTracking()
            .Include(p => p.Membresia).ThenInclude(m => m!.Socio)
            .Include(p => p.Membresia).ThenInclude(m => m!.TipoMembresia);

    public async Task<List<Pago>> BuscarAsync(string? texto, string? estado, DateOnly? desde, DateOnly? hasta)
    {
        var consulta = ConDetalle();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var termino = texto.Trim();
            consulta = consulta.Where(p =>
                p.NumeroRecibo.Contains(termino) ||
                p.Membresia!.Socio!.NombreCompleto.Contains(termino) ||
                p.Membresia!.Socio!.Codigo.Contains(termino));
        }

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(p => p.Estado == estado);

        if (desde.HasValue)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(p => p.FechaHoraCobro >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(p => p.FechaHoraCobro <= fin);
        }

        return await consulta.OrderByDescending(p => p.FechaHoraCobro)
            .ThenByDescending(p => p.Id)
            .ToListAsync();
    }

    public async Task<Pago?> GetConDetalleAsync(int id) =>
        await ConDetalle().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<ResumenPagosDiaDto> ObtenerResumenDelDiaAsync(DateOnly fecha)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue);
        var fin = fecha.ToDateTime(TimeOnly.MaxValue);
        var consulta = _dbSet.AsNoTracking().Where(p =>
            p.Estado == EstadosNegocio.PagoRegistrado &&
            p.FechaHoraCobro >= inicio && p.FechaHoraCobro <= fin);

        var cantidad = await consulta.CountAsync();
        var monto = await consulta.SumAsync(p => (decimal?)p.Monto) ?? 0m;
        return new ResumenPagosDiaDto(cantidad, monto);
    }

    public async Task<bool> ExistePagoValidoAsync(int membresiaId) =>
        await _dbSet.AnyAsync(p =>
            p.MembresiaId == membresiaId && p.Estado == EstadosNegocio.PagoRegistrado);

    public async Task<string> GenerarSiguienteNumeroReciboAsync()
    {
        var ultimoId = await _dbSet.MaxAsync(p => (int?)p.Id) ?? 0;
        return $"P-{ultimoId + 1:0000}";
    }
}
