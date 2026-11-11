using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>Consultas propias de Socio que el repositorio genérico no cubre.</summary>
public interface ISocioRepository : IRepository<Socio>
{
    Task<List<Socio>> BuscarAsync(string? texto);
    Task<Socio?> GetConMembresiasAsync(int id);
    Task<string> GenerarSiguienteCodigoAsync();
}
