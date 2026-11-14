using System.Text.RegularExpressions;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

/// <summary>
/// Reglas de negocio de Proveedores. Habla solo con su repositorio y la unidad de trabajo.
/// </summary>
public class ProveedorService : IProveedorService
{
    private readonly IProveedorRepository _proveedores;
    private readonly IUnitOfWork _unitOfWork;

    public ProveedorService(IProveedorRepository proveedores, IUnitOfWork unitOfWork)
    {
        _proveedores = proveedores;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProveedorDto>> GetAllAsync(string? busqueda = null)
    {
        var proveedores = await _proveedores.BuscarAsync(busqueda);
        return proveedores.Select(p => ADto(p)).ToList();
    }

    public async Task<ProveedorDto?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        var proveedor = await _proveedores.GetConComprasAsync(id);
        return proveedor is null ? null : ADto(proveedor, incluirCompras: true);
    }

    public async Task<ProveedorDto> CreateAsync(ProveedorCreateViewModel modelo)
    {
        var nombre = NormalizarNombre(modelo.Nombre);

        if (await _proveedores.ExisteNombreAsync(nombre))
            throw new InvalidOperationException($"Ya existe un proveedor llamado \"{nombre}\".");

        var proveedor = new Proveedor
        {
            Nombre = nombre,
            Telefono = modelo.Telefono.Trim(),
            Correo = NormalizarCorreo(modelo.Correo)
        };

        await _proveedores.AddAsync(proveedor);
        await _unitOfWork.SaveChangesAsync();
        return ADto(proveedor);
    }

    public async Task<bool> UpdateAsync(ProveedorEditViewModel modelo)
    {
        var proveedor = await _proveedores.GetByIdAsync(modelo.Id);
        if (proveedor is null) return false;

        var nombre = NormalizarNombre(modelo.Nombre);

        if (await _proveedores.ExisteNombreAsync(nombre, excluirId: proveedor.Id))
            throw new InvalidOperationException($"Ya existe otro proveedor llamado \"{nombre}\".");

        proveedor.Nombre = nombre;
        proveedor.Telefono = modelo.Telefono.Trim();
        proveedor.Correo = NormalizarCorreo(modelo.Correo);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var proveedor = await _proveedores.GetByIdAsync(id);
        if (proveedor is null) return false;

        if (await _proveedores.TieneComprasAsync(id))
            throw new InvalidOperationException(
                $"No se puede eliminar a {proveedor.Nombre} porque tiene compras registradas. " +
                "Las compras se conservan como historial.");

        _proveedores.Remove(proveedor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    /// <summary>Quita espacios al inicio, al final y los dobles entre palabras.</summary>
    private static string NormalizarNombre(string nombre) =>
        Regex.Replace(nombre.Trim(), @"\s+", " ");

    private static string? NormalizarCorreo(string? correo) =>
        string.IsNullOrWhiteSpace(correo) ? null : correo.Trim().ToLowerInvariant();

    private static ProveedorDto ADto(Proveedor p, bool incluirCompras = false) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Telefono = p.Telefono,
        Correo = p.Correo,
        CantidadCompras = p.Compras.Count,
        UltimaCompra = p.Compras.Count == 0 ? null : p.Compras.Max(c => c.Fecha),
        Compras = !incluirCompras
            ? new List<CompraDto>()
            : p.Compras
                .OrderByDescending(c => c.Fecha)
                .Select(c => new CompraDto
                {
                    Id = c.Id,
                    ProveedorId = p.Id,
                    ProveedorNombre = p.Nombre,
                    Fecha = c.Fecha,
                    Estado = c.Estado,
                    UsuarioResponsableId = c.UsuarioResponsableId,
                    Lineas = c.Detalles.Select(d => new DetalleCompraDto
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        CostoUnitario = d.CostoUnitario
                    }).ToList()
                })
                .ToList()
    };
}
