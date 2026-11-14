namespace GYMELITEGiTHub.Dtos;

public record ResumenReporteDto(
    decimal IngresosMembresias,
    decimal IngresosProductos,
    int CantidadPagos,
    int CantidadVentas)
{
    public decimal IngresosTotales => IngresosMembresias + IngresosProductos;
    public decimal TicketPromedio => CantidadVentas == 0 ? 0m : IngresosProductos / CantidadVentas;
}

public record IngresoDiaDto(DateOnly Fecha, decimal Membresias, decimal Productos)
{
    public decimal Total => Membresias + Productos;
}

public record IngresoModalidadDto(string Modalidad, int CantidadPagos, decimal Monto);

public record ProductoVendidoDto(
    int ProductoId, string Codigo, string Nombre, long Unidades, decimal Importe);

public record MovimientoProductoDto(
    int ProductoId, string Codigo, string Nombre, long Entradas, long Salidas, int ExistenciaActual);

public record ExistenciaCriticaDto(
    int ProductoId, string Codigo, string Nombre, int Existencias, int StockMinimo);
