using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

public class CompraRepository : Repository<Compra>, ICompraRepository
{
    public CompraRepository(GymEliteDbContext context) : base(context) { }

    private IQueryable<Compra> ConDetalle() =>
        _dbSet.AsNoTracking()
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto);

    public async Task<List<Compra>> BuscarAsync(int? proveedorId, string? estado, DateOnly? desde, DateOnly? hasta)
    {
        var consulta = ConDetalle();

        if (proveedorId is > 0)
            consulta = consulta.Where(c => c.ProveedorId == proveedorId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(c => c.Estado == estado);

        if (desde.HasValue)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(c => c.Fecha >= inicio);
        }

        if (hasta.HasValue)
        {
            var fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(c => c.Fecha <= fin);
        }

        return await consulta
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.Id)
            .ToListAsync();
    }

    public async Task<Compra?> GetConDetalleAsync(int id) =>
        await ConDetalle().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Compra?> GetParaAnularAsync(int id) =>
        await _dbSet.Include(c => c.Detalles).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ResumenComprasDto> ObtenerResumenAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);

        var compras = _dbSet.AsNoTracking().Where(c =>
            c.Estado == EstadosNegocio.CompraRegistrada &&
            c.Fecha >= inicio && c.Fecha <= fin);

        var cantidad = await compras.CountAsync();

        // Join entre DetalleCompra y Compra: el total no se guarda, se calcula.
        var total = await (
            from d in _context.DetallesCompra.AsNoTracking()
            join c in compras on d.CompraId equals c.Id
            select (decimal?)(d.Cantidad * d.CostoUnitario)).SumAsync() ?? 0m;

        return new ResumenComprasDto(cantidad, total);
    }
}
