using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _productos;
    private readonly IInventarioService _inventario;
    private readonly IUnitOfWork _unitOfWork;

    public ProductoService(IProductoRepository productos, IInventarioService inventario, IUnitOfWork unitOfWork)
    {
        _productos = productos;
        _inventario = inventario;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductoListaViewModel> GetListaAsync(string? busqueda, string? categoria)
    {
        var productos = await _productos.BuscarAsync(busqueda, categoria);
        var todos = await _productos.GetAllAsync();
        return new ProductoListaViewModel
        {
            Busqueda = busqueda,
            Categoria = categoria,
            Productos = productos.Select(p => ADto(p)).ToList(),
            ProductosActivos = todos.Count(p => p.Activo),
            ProductosStockBajo = todos.Count(p => p.Activo && p.Existencias <= p.StockMinimo),
            UnidadesDisponibles = todos.Where(p => p.Activo).Sum(p => p.Existencias)
        };
    }

    public async Task<ProductoDto?> GetByIdAsync(int id)
    {
        var producto = await _productos.GetConMovimientosAsync(id);
        return producto is null ? null : ADto(producto, true);
    }

    public async Task<ProductoDto> CreateAsync(ProductoCreateViewModel modelo)
    {
        var codigo = modelo.Codigo.Trim().ToUpperInvariant();
        if (await _productos.ExisteCodigoAsync(codigo))
            throw new InvalidOperationException("Ya existe un producto con ese código.");
        if (!ProductoCreateViewModel.Categorias.Contains(modelo.Categoria))
            throw new InvalidOperationException("La categoría seleccionada no es válida.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var producto = new Producto
            {
                Codigo = codigo,
                Nombre = modelo.Nombre.Trim(),
                Categoria = modelo.Categoria,
                PrecioVenta = modelo.PrecioVenta,
                StockMinimo = modelo.StockMinimo,
                Existencias = 0,
                Activo = true
            };
            await _productos.AddAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            if (modelo.ExistenciaInicial > 0)
            {
                await _inventario.RegistrarEntradaAsync(producto.Id, modelo.ExistenciaInicial,
                    "Registro inicial", "Registro inicial", producto.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();
            return ADto(producto);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(ProductoEditViewModel modelo)
    {
        var producto = await _productos.GetByIdAsync(modelo.Id);
        if (producto is null) return false;
        if (!ProductoCreateViewModel.Categorias.Contains(modelo.Categoria))
            throw new InvalidOperationException("La categoría seleccionada no es válida.");

        producto.Nombre = modelo.Nombre.Trim();
        producto.Categoria = modelo.Categoria;
        producto.PrecioVenta = modelo.PrecioVenta;
        producto.StockMinimo = modelo.StockMinimo;
        producto.Activo = modelo.Activo;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AjustarAsync(ProductoAjusteViewModel modelo)
    {
        if (!await _productos.AnyAsync(p => p.Id == modelo.ProductoId)) return false;

        if (modelo.Tipo == EstadosNegocio.MovimientoEntrada)
            await _inventario.RegistrarEntradaAsync(modelo.ProductoId, modelo.Cantidad, modelo.Motivo, "Ajuste", null);
        else if (modelo.Tipo == EstadosNegocio.MovimientoSalida)
            await _inventario.RegistrarSalidaAsync(modelo.ProductoId, modelo.Cantidad, modelo.Motivo, "Ajuste", null);
        else
            throw new InvalidOperationException("El tipo de ajuste no es válido.");

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var producto = await _productos.GetByIdAsync(id);
        if (producto is null) return false;
        producto.Activo = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static ProductoDto ADto(Producto p, bool incluirMovimientos = false) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        Nombre = p.Nombre,
        Categoria = p.Categoria,
        PrecioVenta = p.PrecioVenta,
        Existencias = p.Existencias,
        StockMinimo = p.StockMinimo,
        Activo = p.Activo,
        Movimientos = incluirMovimientos
            ? p.Movimientos.OrderByDescending(m => m.FechaMovimiento).Select(m => new MovimientoInventarioDto
            {
                Id = m.Id,
                Tipo = m.Tipo,
                Cantidad = m.Cantidad,
                FechaMovimiento = m.FechaMovimiento,
                Motivo = m.Motivo,
                OperacionOrigen = m.OperacionOrigen
            }).ToList()
            : new()
    };
}
