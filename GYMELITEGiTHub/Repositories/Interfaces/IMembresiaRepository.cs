using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>Consultas propias de Membresia (con socio, plan y pagos incluidos).</summary>
public interface IMembresiaRepository : IRepository<Membresia>
{
    Task<List<Membresia>> BuscarAsync(string? texto, string? estado, DateOnly hoy);
    Task<Membresia?> GetConDetalleAsync(int id);

    /// <summary>Versión con seguimiento (tracked) que incluye pagos y accesos, para poder cancelar.</summary>
    Task<Membresia?> GetParaCancelarAsync(int id);

    Task<List<Membresia>> GetPorSocioAsync(int socioId);

    /// <summary>Última membresía no cancelada del socio.</summary>
    Task<Membresia?> GetUltimaDeSocioAsync(int socioId);

    /// <summary>Membresía que da acceso en la fecha: no cancelada, con pago válido y dentro de sus fechas.</summary>
    Task<Membresia?> GetVigenteAsync(int socioId, DateOnly fecha);

    /// <summary>Hay traslape con otra membresía no cancelada del mismo socio.</summary>
    Task<bool> ExisteTraslapeAsync(int socioId, DateOnly inicio, DateOnly fin);
}
