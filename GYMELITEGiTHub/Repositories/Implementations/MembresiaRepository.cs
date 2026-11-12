using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class MembresiaRepository : Repository<Membresia>, IMembresiaRepository
{
    public MembresiaRepository(GymEliteDbContext context) : base(context) { }

    private IQueryable<Membresia> ConSocioPlanYPagos() =>
        _dbSet.AsNoTracking()
              .Include(m => m.Socio)
              .Include(m => m.TipoMembresia)
              .Include(m => m.Pagos);

    public async Task<List<Membresia>> BuscarAsync(string? texto, string? estado, DateOnly hoy)
    {
        var consulta = ConSocioPlanYPagos();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(m =>
                m.Socio!.NombreCompleto.Contains(t) ||
                m.Socio!.Codigo.Contains(t) ||
                m.TipoMembresia!.Nombre.Contains(t));
        }

        var limite = hoy.AddDays(7);
        consulta = estado switch
        {
            "vigentes" => consulta.Where(m =>
                m.Estado != EstadosNegocio.MembresiaCancelada &&
                m.Pagos.Any(p => p.Estado == EstadosNegocio.PagoRegistrado) &&
                m.FechaInicio <= hoy && m.FechaFin >= hoy),

            "porvencer" => consulta.Where(m =>
                m.Estado != EstadosNegocio.MembresiaCancelada &&
                m.Pagos.Any(p => p.Estado == EstadosNegocio.PagoRegistrado) &&
                m.FechaInicio <= hoy && m.FechaFin >= hoy && m.FechaFin <= limite),

            "pendientes" => consulta.Where(m =>
                m.Estado != EstadosNegocio.MembresiaCancelada &&
                !m.Pagos.Any(p => p.Estado == EstadosNegocio.PagoRegistrado) &&
                m.FechaFin >= hoy),

            "vencidas" => consulta.Where(m =>
                m.Estado != EstadosNegocio.MembresiaCancelada && m.FechaFin < hoy),

            "programadas" => consulta.Where(m =>
                m.Estado != EstadosNegocio.MembresiaCancelada &&
                m.Pagos.Any(p => p.Estado == EstadosNegocio.PagoRegistrado) &&
                m.FechaInicio > hoy),

            "canceladas" => consulta.Where(m => m.Estado == EstadosNegocio.MembresiaCancelada),

            _ => consulta
        };

        return await consulta.OrderByDescending(m => m.FechaFin).ToListAsync();
    }

    public async Task<Membresia?> GetConDetalleAsync(int id) =>
        await ConSocioPlanYPagos().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Membresia?> GetParaCancelarAsync(int id) =>
        await _dbSet
            .Include(m => m.Socio)
            .Include(m => m.TipoMembresia)
            .Include(m => m.Pagos)
            .Include(m => m.Accesos)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Membresia>> GetPorSocioAsync(int socioId) =>
        await ConSocioPlanYPagos()
            .Where(m => m.SocioId == socioId)
            .OrderByDescending(m => m.FechaFin)
            .ToListAsync();

    public async Task<Membresia?> GetUltimaDeSocioAsync(int socioId) =>
        await _dbSet.AsNoTracking()
            .Where(m => m.SocioId == socioId && m.Estado != EstadosNegocio.MembresiaCancelada)
            .OrderByDescending(m => m.FechaFin)
            .FirstOrDefaultAsync();

    public async Task<Membresia?> GetVigenteAsync(int socioId, DateOnly fecha) =>
        await ConSocioPlanYPagos()
            .Where(m => m.SocioId == socioId
                        && m.Estado != EstadosNegocio.MembresiaCancelada
                        && m.Pagos.Any(p => p.Estado == EstadosNegocio.PagoRegistrado)
                        && m.FechaInicio <= fecha && m.FechaFin >= fecha)
            .OrderByDescending(m => m.FechaFin)
            .FirstOrDefaultAsync();

    public async Task<bool> ExisteTraslapeAsync(int socioId, DateOnly inicio, DateOnly fin) =>
        await _dbSet.AnyAsync(m =>
            m.SocioId == socioId
            && m.Estado != EstadosNegocio.MembresiaCancelada
            && m.FechaInicio <= fin && m.FechaFin >= inicio);
}
