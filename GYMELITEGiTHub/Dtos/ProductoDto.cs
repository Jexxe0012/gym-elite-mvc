namespace GYMELITEGiTHub.Dtos;

public class ProductoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public int Existencias { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; }
    public string EstadoExistencias => Existencias <= 0
        ? "Agotado"
        : Existencias <= StockMinimo ? "Stock bajo" : "Disponible";
    public List<MovimientoInventarioDto> Movimientos { get; set; } = new();
}

public class MovimientoInventarioDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string OperacionOrigen { get; set; } = string.Empty;
}
