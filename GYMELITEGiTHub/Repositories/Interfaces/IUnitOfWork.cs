namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>
/// Unidad de trabajo: confirma en una sola transacción todo lo que los
/// repositorios marcaron (por ejemplo, una venta + sus detalles + el inventario).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
