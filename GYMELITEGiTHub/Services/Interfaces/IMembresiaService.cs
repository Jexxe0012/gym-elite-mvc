using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IMembresiaService
{
    Task<List<MembresiaDto>> GetAllAsync(string? busqueda = null, string? estado = null);
    Task<MembresiaDto?> GetByIdAsync(int id);

    /// <summary>Todas las membresías del socio, de la más reciente a la más antigua (incluye canceladas).</summary>
    Task<List<MembresiaDto>> GetPorSocioAsync(int socioId);

    /// <summary>
    /// Membresía que da acceso en la fecha indicada (hoy por defecto). Solo cuenta si no está
    /// cancelada y tiene un pago válido. La usará Accesos; devuelve null si no hay cobertura.
    /// </summary>
    Task<MembresiaDto?> GetVigenteDeSocioAsync(int socioId, DateOnly? fecha = null);

    /// <summary>
    /// Registra una membresía. Con CobrarAhora registra también el pago en efectivo,
    /// en la misma operación. Lanza InvalidOperationException si incumple una regla.
    /// </summary>
    Task<MembresiaDto> CreateAsync(MembresiaCreateViewModel modelo);

    /// <summary>
    /// Cancela la membresía y conserva el historial. Si tiene pago válido, lo anula.
    /// Devuelve null si no existe. Lanza InvalidOperationException si no se puede cancelar.
    /// </summary>
    Task<ResultadoCancelacion?> CancelarAsync(int id);

    Task<MembresiaCreateViewModel> PrepararNuevaAsync();

    /// <summary>Formulario precargado para renovar. Null si el socio no existe.</summary>
    Task<MembresiaCreateViewModel?> PrepararRenovacionAsync(int socioId);

    Task CargarOpcionesAsync(MembresiaCreateViewModel modelo);
}
