using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

public class VentaService : IVentaService
{
    private readonly IVentaRepository _ventas;
    private readonly IProductoRepository _productos;
    private readonly IRepository<DetalleVenta> _detalles;
    private readonly IInventarioService _inventario;
    private readonly IUnitOfWork _unitOfWork;

    public VentaService(IVentaRepository ventas, IProductoRepository productos,
        IRepository<DetalleVenta> detalles, IInventarioService inventario, IUnitOfWork unitOfWork)
    {
        _ventas = ventas;
        _productos = productos;
        _detalles = detalles;
        _inventario = inventario;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<VentaDto>> GetAllAsync(DateTime? desde, DateTime? hasta) =>
        (await _ventas.BuscarAsync(desde, hasta)).Select(ADto).ToList();

    public async Task<VentaNuevaViewModel> PrepararNuevaAsync() => new()
    {
        Lineas = (await _productos.GetActivosConExistenciasAsync()).Select(p => new VentaLineaViewModel
        {
            ProductoId = p.Id,
            Codigo = p.Codigo,
            Nombre = p.Nombre,
            Precio = p.PrecioVenta,
            Existencias = p.Existencias
        }).ToList()
    };

    public async Task<VentaDto> RegistrarAsync(VentaNuevaViewModel modelo)
    {
        var solicitadas = modelo.Lineas.Where(l => l.Cantidad > 0).ToList();
        if (solicitadas.Count == 0)
            throw new InvalidOperationException("Seleccione al menos un producto e indique una cantidad mayor que cero.");

        var productos = new List<(Producto Producto, int Cantidad)>();
        foreach (var linea in solicitadas)
        {
            var producto = await _productos.GetByIdAsync(linea.ProductoId)
                ?? throw new InvalidOperationException("Uno de los productos seleccionados ya no existe.");
            if (!producto.Activo)
                throw new InvalidOperationException($"El producto {producto.Nombre} está inactivo.");
            if (producto.Existencias < linea.Cantidad)
                throw new InvalidOperationException($"Existencias insuficientes para {producto.Nombre}. Disponibles: {producto.Existencias}.");
            productos.Add((producto, linea.Cantidad));
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Estado = EstadosNegocio.VentaRegistrada,
                UsuarioResponsableId = "sistema",
                Total = productos.Sum(x => x.Producto.PrecioVenta * x.Cantidad)
            };
            await _ventas.AddAsync(venta);
            await _unitOfWork.SaveChangesAsync();

            foreach (var (producto, cantidad) in productos)
            {
                await _detalles.AddAsync(new DetalleVenta
                {
                    VentaId = venta.Id,
                    ProductoId = producto.Id,
                    Cantidad = cantidad,
                    PrecioAplicado = producto.PrecioVenta
                });
                await _inventario.RegistrarSalidaAsync(producto.Id, cantidad,
                    $"Venta {venta.Id}", "Venta", venta.Id);
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return (await GetByIdAsync(venta.Id))!;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<VentaDto?> GetByIdAsync(int id)
    {
        var venta = await _ventas.GetConDetallesAsync(id);
        return venta is null ? null : ADto(venta);
    }

    public async Task<bool> AnularAsync(int id)
    {
        var venta = await _ventas.GetConDetallesAsync(id);
        if (venta is null) return false;
        if (venta.Estado == EstadosNegocio.VentaAnulada)
            throw new InvalidOperationException("La venta ya fue anulada.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var detalle in venta.Detalles)
                await _inventario.RegistrarEntradaAsync(detalle.ProductoId, detalle.Cantidad,
                    $"Anulación de venta {venta.Id}", "Anulación de venta", venta.Id);

            venta.Estado = EstadosNegocio.VentaAnulada;
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static VentaDto ADto(Venta v) => new()
    {
        Id = v.Id,
        Fecha = v.Fecha,
        Total = v.Total,
        Estado = v.Estado,
        Detalles = v.Detalles.Select(d => new DetalleVentaDto
        {
            ProductoId = d.ProductoId,
            CodigoProducto = d.Producto?.Codigo ?? string.Empty,
            NombreProducto = d.Producto?.Nombre ?? string.Empty,
            Cantidad = d.Cantidad,
            PrecioAplicado = d.PrecioAplicado
        }).ToList()
    };
}
