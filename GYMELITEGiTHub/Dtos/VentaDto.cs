namespace GYMELITEGiTHub.Dtos;

public class VentaDto
{
    public int Id { get; set; }
    public string Numero => $"V-{Id:000}";
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<DetalleVentaDto> Detalles { get; set; } = new();
}

public class DetalleVentaDto
{
    public int ProductoId { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioAplicado { get; set; }
    public decimal Subtotal => Cantidad * PrecioAplicado;
}
