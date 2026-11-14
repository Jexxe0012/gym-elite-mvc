namespace GYMELITEGiTHub.Services.Interfaces;

public interface IInventarioService
{
    Task RegistrarEntradaAsync(int productoId, int cantidad, string motivo,
        string operacionOrigen, int? operacionOrigenId);
    Task RegistrarSalidaAsync(int productoId, int cantidad, string motivo,
        string operacionOrigen, int? operacionOrigenId);
}
