using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymEliteDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(GymEliteDbContext context) => _context = context;

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        if (_transaction is null)
            _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
