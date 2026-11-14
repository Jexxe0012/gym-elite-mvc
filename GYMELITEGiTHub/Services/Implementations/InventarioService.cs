using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;

namespace GYMELITEGiTHub.Services.Implementations;

public class InventarioService : IInventarioService
{
    private readonly IProductoRepository _productos;
    private readonly IRepository<MovimientoInventario> _movimientos;

    public InventarioService(IProductoRepository productos, IRepository<MovimientoInventario> movimientos)
    {
        _productos = productos;
        _movimientos = movimientos;
    }

    public Task RegistrarEntradaAsync(int productoId, int cantidad, string motivo,
        string operacionOrigen, int? operacionOrigenId) =>
        RegistrarAsync(productoId, cantidad, motivo, operacionOrigen, operacionOrigenId, true);

    public Task RegistrarSalidaAsync(int productoId, int cantidad, string motivo,
        string operacionOrigen, int? operacionOrigenId) =>
        RegistrarAsync(productoId, cantidad, motivo, operacionOrigen, operacionOrigenId, false);

    private async Task RegistrarAsync(int productoId, int cantidad, string motivo,
        string operacionOrigen, int? operacionOrigenId, bool entrada)
    {
        if (cantidad <= 0) throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new InvalidOperationException("El motivo es obligatorio.");

        var producto = await _productos.GetByIdAsync(productoId)
            ?? throw new InvalidOperationException("El producto indicado no existe.");

        if (!entrada && producto.Existencias < cantidad)
            throw new InvalidOperationException($"Existencias insuficientes para {producto.Nombre}. Disponibles: {producto.Existencias}.");

        producto.Existencias += entrada ? cantidad : -cantidad;
        await _movimientos.AddAsync(new MovimientoInventario
        {
            ProductoId = productoId,
            Tipo = entrada ? EstadosNegocio.MovimientoEntrada : EstadosNegocio.MovimientoSalida,
            Cantidad = cantidad,
            FechaMovimiento = DateTime.Now,
            Motivo = motivo.Trim(),
            UsuarioResponsableId = "sistema",
            OperacionOrigen = operacionOrigen,
            OperacionOrigenId = operacionOrigenId
        });
    }
}
