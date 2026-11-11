using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Repositories.Interfaces;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymEliteDbContext _context;

    public UnitOfWork(GymEliteDbContext context) => _context = context;

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
