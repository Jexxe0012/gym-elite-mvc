using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.Services.Implementations;

/// <summary>
/// Reglas de negocio de Compras. La regla de existencias vive en IInventarioService
/// (Integrante C); este servicio solo la usa, no la duplica.
/// </summary>
public class CompraService : ICompraService
{
    private const string UsuarioSistema = "sistema";
    private const string OrigenCompra = "Compra";
    private const string MotivoEntrada = "Entrada por compra";
    private const string MotivoAnulacion = "Anulación de compra";

    private readonly ICompraRepository _compras;
    private readonly IProveedorRepository _proveedores;
    private readonly IRepository<Producto> _productos;
    private readonly IInventarioService _inventario;
    private readonly IUnitOfWork _unitOfWork;

    public CompraService(
        ICompraRepository compras,
        IProveedorRepository proveedores,
        IRepository<Producto> productos,
        IInventarioService inventario,
        IUnitOfWork unitOfWork)
    {
        _compras = compras;
        _proveedores = proveedores;
        _productos = productos;
        _inventario = inventario;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompraListaViewModel> ObtenerListadoAsync(CompraListaViewModel filtros)
    {
        if (filtros.FechaDesde.HasValue && filtros.FechaHasta.HasValue &&
            filtros.FechaDesde.Value > filtros.FechaHasta.Value)
            throw new InvalidOperationException("La fecha inicial no puede ser posterior a la final.");

        if (!string.IsNullOrWhiteSpace(filtros.Estado) &&
            filtros.Estado != EstadosNegocio.CompraRegistrada && filtros.Estado != EstadosNegocio.CompraAnulada)
            throw new InvalidOperationException("Seleccione un estado de compra válido.");

        var compras = await _compras.BuscarAsync(filtros.ProveedorId, filtros.Estado, filtros.FechaDesde, filtros.FechaHasta);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);

        filtros.Compras = compras.Select(ADto).ToList();
        filtros.ResumenMes = await _compras.ObtenerResumenAsync(inicioMes, finMes);
        filtros.Proveedores = await ListaProveedoresAsync(filtros.ProveedorId);
        return filtros;
    }

    public async Task<CompraDto?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        var compra = await _compras.GetConDetalleAsync(id);
        return compra is null ? null : ADto(compra);
    }

    public async Task<CompraNuevaViewModel> PrepararNuevaAsync(int? proveedorId = null)
    {
        var modelo = new CompraNuevaViewModel
        {
            ProveedorId = proveedorId,
            Lineas = new List<LineaCompraViewModel> { new() }
        };
        await CargarOpcionesAsync(modelo);
        return modelo;
    }

    public async Task CargarOpcionesAsync(CompraNuevaViewModel modelo)
    {
        if (modelo.Lineas.Count == 0) modelo.Lineas.Add(new LineaCompraViewModel());

        modelo.Proveedores = await ListaProveedoresAsync(modelo.ProveedorId);

        var activos = await _productos.FindAsync(p => p.Activo);
        modelo.Productos = activos
            .OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem($"{p.Codigo} · {p.Nombre} (existencias: {p.Existencias})", p.Id.ToString()))
            .ToList();
    }

    public async Task<CompraDto> RegistrarAsync(CompraNuevaViewModel modelo)
    {
        // 1. Validar todo antes de modificar nada.
        if (modelo.ProveedorId is null or <= 0)
            throw new InvalidOperationException("Seleccione un proveedor.");

        var proveedor = await _proveedores.GetByIdAsync(modelo.ProveedorId.Value)
            ?? throw new InvalidOperationException("El proveedor seleccionado no existe.");

        var lineas = modelo.Lineas.Where(l => !l.EstaVacia).ToList();
        if (lineas.Count == 0)
            throw new InvalidOperationException("Agregue al menos una línea con producto, cantidad y costo.");

        if (lineas.Where(l => l.ProductoId > 0).GroupBy(l => l.ProductoId).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Un producto aparece en más de una línea. Combine las cantidades en una sola línea.");

        var detalles = new List<DetalleCompra>();
        foreach (var (linea, numero) in lineas.Select((l, i) => (l, i + 1)))
        {
            if (linea.ProductoId is null or <= 0)
                throw new InvalidOperationException($"Línea {numero}: seleccione un producto.");
            if (linea.Cantidad is null or <= 0)
                throw new InvalidOperationException($"Línea {numero}: la cantidad debe ser mayor que cero.");
            if (linea.CostoUnitario is null or <= 0m)
                throw new InvalidOperationException($"Línea {numero}: el costo unitario debe ser mayor que cero.");

            var producto = await _productos.GetByIdAsync(linea.ProductoId.Value)
                ?? throw new InvalidOperationException($"Línea {numero}: el producto seleccionado no existe.");

            if (!producto.Activo)
                throw new InvalidOperationException($"Línea {numero}: el producto {producto.Nombre} está inactivo.");

            detalles.Add(new DetalleCompra
            {
                ProductoId = producto.Id,
                Cantidad = linea.Cantidad.Value,
                CostoUnitario = decimal.Round(linea.CostoUnitario.Value, 2)
            });
        }

        var compra = new Compra
        {
            ProveedorId = proveedor.Id,
            Fecha = DateTime.Now,
            Estado = EstadosNegocio.CompraRegistrada,
            UsuarioResponsableId = UsuarioSistema,
            Detalles = detalles
        };

        // 2. Guardar compra + detalles y, con el Id ya generado, las entradas de inventario.
        //    Todo dentro de una transacción: si algo falla, no queda nada a medias.
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _compras.AddAsync(compra);
            await _unitOfWork.SaveChangesAsync();

            foreach (var detalle in detalles)
            {
                await _inventario.RegistrarEntradaAsync(
                    detalle.ProductoId, detalle.Cantidad, MotivoEntrada, OrigenCompra, compra.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return await GetByIdAsync(compra.Id)
            ?? throw new InvalidOperationException("La compra se guardó, pero no se pudo consultar.");
    }

    public async Task<bool> AnularAsync(int id)
    {
        if (id <= 0) return false;

        var compra = await _compras.GetParaAnularAsync(id);
        if (compra is null) return false;

        if (compra.Estado == EstadosNegocio.CompraAnulada)
            throw new InvalidOperationException("La compra ya está anulada.");
        if (compra.Estado != EstadosNegocio.CompraRegistrada)
            throw new InvalidOperationException("Solo se puede anular una compra registrada.");

        // Revisar primero que revertir las entradas no deje existencias negativas
        // (por ejemplo, si ya se vendió parte de lo comprado).
        var porProducto = compra.Detalles
            .GroupBy(d => d.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(d => d.Cantidad) })
            .ToList();

        foreach (var item in porProducto)
        {
            var producto = await _productos.GetByIdAsync(item.ProductoId)
                ?? throw new InvalidOperationException("Uno de los productos de la compra ya no existe.");

            if (producto.Existencias < item.Cantidad)
                throw new InvalidOperationException(
                    $"No se puede anular: de {producto.Nombre} quedan {producto.Existencias} unidad(es) " +
                    $"y la compra ingresó {item.Cantidad}. Parte de lo comprado ya salió del inventario.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var item in porProducto)
            {
                await _inventario.RegistrarSalidaAsync(
                    item.ProductoId, item.Cantidad, MotivoAnulacion, OrigenCompra, compra.Id);
            }

            compra.Estado = EstadosNegocio.CompraAnulada;
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return true;
    }

    private async Task<List<SelectListItem>> ListaProveedoresAsync(int? seleccionado)
    {
        var proveedores = await _proveedores.GetAllAsync();
        return proveedores
            .OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem(p.Nombre, p.Id.ToString(), p.Id == seleccionado))
            .ToList();
    }

    private static CompraDto ADto(Compra c) => new()
    {
        Id = c.Id,
        ProveedorId = c.ProveedorId,
        ProveedorNombre = c.Proveedor?.Nombre ?? string.Empty,
        Fecha = c.Fecha,
        Estado = c.Estado,
        UsuarioResponsableId = c.UsuarioResponsableId,
        Lineas = c.Detalles
            .OrderBy(d => d.Id)
            .Select(d => new DetalleCompraDto
            {
                ProductoId = d.ProductoId,
                ProductoCodigo = d.Producto?.Codigo ?? string.Empty,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                Cantidad = d.Cantidad,
                CostoUnitario = d.CostoUnitario
            })
            .ToList()
    };
}
