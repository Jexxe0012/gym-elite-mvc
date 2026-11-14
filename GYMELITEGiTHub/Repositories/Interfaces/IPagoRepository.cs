using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>
/// Consultas del historial y de los comprobantes. No confirma cambios.
/// </summary>
public interface IPagoRepository : IRepository<Pago>
{
    /// <summary>True si la membresía ya tiene un pago con estado "Registrado".</summary>
    Task<bool> ExistePagoValidoAsync(int membresiaId);

    /// <summary>Siguiente número de recibo con formato P-0001.</summary>
    Task<string> GenerarSiguienteNumeroReciboAsync();

    Task<List<Pago>> BuscarAsync(string? texto, string? estado, DateOnly? desde, DateOnly? hasta);
    Task<Pago?> GetConDetalleAsync(int id);
    Task<ResumenPagosDiaDto> ObtenerResumenDelDiaAsync(DateOnly fecha);
}
