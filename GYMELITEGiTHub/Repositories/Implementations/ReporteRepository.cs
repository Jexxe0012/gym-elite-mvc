using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Repositories.Implementations;

/// <summary>Consultas de lectura para reportes; no modifica entidades ni confirma cambios.</summary>
public class ReporteRepository : IReporteRepository
{
    private readonly GymEliteDbContext _context;

    public ReporteRepository(GymEliteDbContext context) => _context = context;

    public async Task<ResumenReporteDto> ObtenerResumenAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);
        var pagos = _context.Pagos.AsNoTracking().Where(p =>
            p.Estado == EstadosNegocio.PagoRegistrado && p.FechaHoraCobro >= inicio && p.FechaHoraCobro <= fin);
        var ventas = _context.Ventas.AsNoTracking().Where(v =>
            v.Estado == EstadosNegocio.VentaRegistrada && v.Fecha >= inicio && v.Fecha <= fin);

        var ingresoMembresias = await pagos.SumAsync(p => (decimal?)p.Monto) ?? 0m;
        var ingresoProductos = await ventas.SumAsync(v => (decimal?)v.Total) ?? 0m;
        var cantidadPagos = await pagos.CountAsync();
        var cantidadVentas = await ventas.CountAsync();
        return new ResumenReporteDto(ingresoMembresias, ingresoProductos, cantidadPagos, cantidadVentas);
    }

    public async Task<List<IngresoDiaDto>> ObtenerIngresosPorDiaAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);
        var pagos = await _context.Pagos.AsNoTracking()
            .Where(p => p.Estado == EstadosNegocio.PagoRegistrado
                && p.FechaHoraCobro >= inicio && p.FechaHoraCobro <= fin)
            .GroupBy(p => p.FechaHoraCobro.Date)
            .Select(g => new { Fecha = g.Key, Monto = g.Sum(p => p.Monto) })
            .ToListAsync();
        var ventas = await _context.Ventas.AsNoTracking()
            .Where(v => v.Estado == EstadosNegocio.VentaRegistrada && v.Fecha >= inicio && v.Fecha <= fin)
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new { Fecha = g.Key, Monto = g.Sum(v => v.Total) })
            .ToListAsync();

        // Las agrupaciones se ejecutan en SQL. El servicio combina ambas fuentes y agrega días sin movimientos.
        return pagos.Select(p => new IngresoDiaDto(DateOnly.FromDateTime(p.Fecha), p.Monto, 0m))
            .Concat(ventas.Select(v => new IngresoDiaDto(DateOnly.FromDateTime(v.Fecha), 0m, v.Monto)))
            .OrderBy(d => d.Fecha)
            .ToList();
    }

    public async Task<List<IngresoModalidadDto>> ObtenerIngresosPorModalidadAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);
        var filas = await (
            from pago in _context.Pagos.AsNoTracking()
            join membresia in _context.Membresias.AsNoTracking() on pago.MembresiaId equals membresia.Id
            join tipo in _context.TiposMembresia.AsNoTracking() on membresia.TipoMembresiaId equals tipo.Id
            where pago.Estado == EstadosNegocio.PagoRegistrado
                && pago.FechaHoraCobro >= inicio && pago.FechaHoraCobro <= fin
            group pago by tipo.Modalidad into grupo
            orderby grupo.Key
            select new { Modalidad = grupo.Key, Cantidad = grupo.Count(), Monto = grupo.Sum(p => p.Monto) })
            .ToListAsync();

        return filas.Select(f => new IngresoModalidadDto(f.Modalidad, f.Cantidad, f.Monto)).ToList();
    }

    public async Task<List<ProductoVendidoDto>> ObtenerProductosMasVendidosAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);
        var filas = await (
            from detalle in _context.DetallesVenta.AsNoTracking()
            join venta in _context.Ventas.AsNoTracking() on detalle.VentaId equals venta.Id
            join producto in _context.Productos.AsNoTracking() on detalle.ProductoId equals producto.Id
            where venta.Estado == EstadosNegocio.VentaRegistrada && venta.Fecha >= inicio && venta.Fecha <= fin
            group detalle by new { producto.Id, producto.Codigo, producto.Nombre } into grupo
            select new
            {
                grupo.Key.Id,
                grupo.Key.Codigo,
                grupo.Key.Nombre,
                Unidades = grupo.Sum(d => (long)d.Cantidad),
                Importe = grupo.Sum(d => d.Cantidad * d.PrecioAplicado)
            })
            .OrderByDescending(f => f.Unidades)
            .ThenBy(f => f.Nombre)
            .ThenBy(f => f.Id)
            .Take(5)
            .ToListAsync();

        return filas.Select(f => new ProductoVendidoDto(f.Id, f.Codigo, f.Nombre, f.Unidades, f.Importe)).ToList();
    }

    public async Task<List<MovimientoProductoDto>> ObtenerMovimientosPorProductoAsync(DateOnly desde, DateOnly hasta)
    {
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.ToDateTime(TimeOnly.MaxValue);
        var movimientosDelPeriodo = _context.MovimientosInventario.AsNoTracking()
            .Where(m => m.FechaMovimiento >= inicio && m.FechaMovimiento <= fin);

        var filas = await (
            from producto in _context.Productos.AsNoTracking()
            join movimiento in movimientosDelPeriodo on producto.Id equals movimiento.ProductoId into movimientos
            from movimiento in movimientos.DefaultIfEmpty()
            group movimiento by new { producto.Id, producto.Codigo, producto.Nombre, producto.Existencias } into grupo
            orderby grupo.Key.Nombre, grupo.Key.Id
            select new
            {
                grupo.Key.Id,
                grupo.Key.Codigo,
                grupo.Key.Nombre,
                grupo.Key.Existencias,
                Entradas = grupo.Sum(m => m != null && m.Tipo == EstadosNegocio.MovimientoEntrada ? (long)m.Cantidad : 0L),
                Salidas = grupo.Sum(m => m != null && m.Tipo == EstadosNegocio.MovimientoSalida ? (long)m.Cantidad : 0L)
            })
            .ToListAsync();

        return filas.Select(f => new MovimientoProductoDto(f.Id, f.Codigo, f.Nombre,
            f.Entradas, f.Salidas, f.Existencias)).ToList();
    }

    public async Task<List<ExistenciaCriticaDto>> ObtenerExistenciasCriticasAsync()
    {
        var filas = await _context.Productos.AsNoTracking()
            .Where(p => p.Activo && p.Existencias <= p.StockMinimo)
            .OrderBy(p => p.Existencias)
            .ThenBy(p => p.Nombre)
            .ThenBy(p => p.Id)
            .Select(p => new { p.Id, p.Codigo, p.Nombre, p.Existencias, p.StockMinimo })
            .ToListAsync();

        return filas.Select(f => new ExistenciaCriticaDto(f.Id, f.Codigo, f.Nombre, f.Existencias, f.StockMinimo)).ToList();
    }
}
