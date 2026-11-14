using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>Consultas propias de Proveedor que el repositorio genérico no cubre.</summary>
public interface IProveedorRepository : IRepository<Proveedor>
{
    /// <summary>Busca por nombre, teléfono o correo e incluye sus compras para contar el historial.</summary>
    Task<List<Proveedor>> BuscarAsync(string? texto);

    /// <summary>Proveedor con sus compras y las líneas de cada una (para el detalle).</summary>
    Task<Proveedor?> GetConComprasAsync(int id);

    /// <summary>True si ya existe otro proveedor con el mismo nombre.</summary>
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);

    /// <summary>True si el proveedor tiene compras registradas o anuladas.</summary>
    Task<bool> TieneComprasAsync(int id);
}
