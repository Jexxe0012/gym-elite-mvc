using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class AccesoRepository : Repository<Acceso>, IAccesoRepository
{
    public AccesoRepository(GymEliteDbContext context) : base(context) { }

    private IQueryable<Acceso> ConDetalle() =>
        _dbSet.AsNoTracking()
            .Include(a => a.Membresia).ThenInclude(m => m!.Socio)
            .Include(a => a.Membresia).ThenInclude(m => m!.TipoMembresia);

    public async Task<List<Acceso>> BuscarAsync(string? texto, DateOnly? desde, DateOnly? hasta, int limite)
    {
        var consulta = ConDetalle();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(a =>
                a.Membresia!.Socio!.NombreCompleto.Contains(t) ||
                a.Membresia!.Socio!.Codigo.Contains(t));
        }

        if (desde.HasValue)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(a => a.FechaHoraIngreso >= inicio);
        }

        if (hasta.HasValue)
        {
            // Hasta el final del día indicado: se compara contra el inicio del día siguiente.
            var finExclusivo = hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(a => a.FechaHoraIngreso < finExclusivo);
        }

        return await consulta
            .OrderByDescending(a => a.FechaHoraIngreso)
            .ThenByDescending(a => a.Id)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<List<Acceso>> GetRecientesAsync(int cantidad) =>
        await ConDetalle()
            .OrderByDescending(a => a.FechaHoraIngreso)
            .ThenByDescending(a => a.Id)
            .Take(cantidad)
            .ToListAsync();

    public async Task<bool> ExisteIngresoRecienteAsync(int membresiaId, DateTime desde) =>
        await _dbSet.AnyAsync(a => a.MembresiaId == membresiaId && a.FechaHoraIngreso >= desde);

    public async Task<DateTime?> GetUltimoIngresoDeSocioAsync(int socioId) =>
        await _dbSet.AsNoTracking()
            .Where(a => a.Membresia!.SocioId == socioId)
            .OrderByDescending(a => a.FechaHoraIngreso)
            .Select(a => (DateTime?)a.FechaHoraIngreso)
            .FirstOrDefaultAsync();

    public async Task<ResumenAccesosDiaDto> ObtenerResumenDelDiaAsync(DateOnly fecha)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue);
        var finExclusivo = fecha.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var consulta = _dbSet.AsNoTracking()
            .Where(a => a.FechaHoraIngreso >= inicio && a.FechaHoraIngreso < finExclusivo);

        var ingresos = await consulta.CountAsync();
        var socios = await consulta.Select(a => a.Membresia!.SocioId).Distinct().CountAsync();
        return new ResumenAccesosDiaDto(ingresos, socios);
    }
}
