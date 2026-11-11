using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>
/// Base del repositorio de pagos. El módulo Pagos lo amplía con sus propias consultas
/// (búsqueda del historial, pagos del día, etc.).
/// </summary>
public interface IPagoRepository : IRepository<Pago>
{
    /// <summary>True si la membresía ya tiene un pago con estado "Registrado".</summary>
    Task<bool> ExistePagoValidoAsync(int membresiaId);

    /// <summary>Siguiente número de recibo con formato P-0001.</summary>
    Task<string> GenerarSiguienteNumeroReciboAsync();
}
