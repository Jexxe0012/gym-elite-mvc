using System.Linq.Expressions;

namespace GYMELITEGiTHub.Repositories.Interfaces;

/// <summary>
/// Contrato genérico de acceso a datos. Los repositorios NO guardan cambios:
/// eso lo hace IUnitOfWork para agrupar varias operaciones en una sola transacción.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> filtro);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filtro);
    Task AddAsync(T entidad);
    void Update(T entidad);
    void Remove(T entidad);
}
