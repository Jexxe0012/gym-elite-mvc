using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IAccesoService
{
    /// <summary>
    /// Llena la pantalla de recepción: últimos ingresos, resumen del día y, si hay búsqueda,
    /// la verificación del socio. Lanza InvalidOperationException si la búsqueda no es válida.
    /// </summary>
    Task PrepararAsync(AccesoVerificarViewModel modelo);

    /// <summary>Consulta si el socio puede ingresar hoy, sin registrar nada.</summary>
    Task<VerificacionAccesoDto> VerificarAsync(string? busqueda);

    /// <summary>
    /// Registra un ingreso autorizado. Vuelve a verificar la cobertura en el servidor.
    /// Lanza InvalidOperationException (con el motivo) si no corresponde registrarlo.
    /// </summary>
    Task<AccesoDto> RegistrarIngresoAsync(int socioId);

    Task<AccesoDto?> GetByIdAsync(int id);
    Task<List<AccesoDto>> GetRecientesAsync(int cantidad = 8);
    Task<AccesoHistorialViewModel> ObtenerHistorialAsync(AccesoHistorialViewModel filtros);
}
