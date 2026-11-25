using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>Consultas propias de Acceso (con membresía, socio y plan incluidos).</summary>
public interface IAccesoRepository : IRepository<Acceso>
{
    Task<List<Acceso>> BuscarAsync(string? texto, DateOnly? desde, DateOnly? hasta, int limite);
    Task<List<Acceso>> GetRecientesAsync(int cantidad);
    Task<Acceso?> GetConDetalleAsync(int id);

    /// <summary>True si la membresía tiene un ingreso registrado desde la fecha y hora indicadas.</summary>
    Task<bool> ExisteIngresoRecienteAsync(int membresiaId, DateTime desde);

    Task<DateTime?> GetUltimoIngresoDeSocioAsync(int socioId);
    Task<ResumenAccesosDiaDto> ObtenerResumenDelDiaAsync(DateOnly fecha);
}
