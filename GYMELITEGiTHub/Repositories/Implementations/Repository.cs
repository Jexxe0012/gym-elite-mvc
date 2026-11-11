using System.Linq.Expressions;
using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GymEliteDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(GymEliteDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filtro) =>
        await _dbSet.AsNoTracking().Where(filtro).ToListAsync();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filtro) =>
        await _dbSet.AnyAsync(filtro);

    public async Task AddAsync(T entidad) => await _dbSet.AddAsync(entidad);

    public void Update(T entidad) => _dbSet.Update(entidad);

    public void Remove(T entidad) => _dbSet.Remove(entidad);
}
