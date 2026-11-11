namespace GYMELITEGiTHub.Models;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Registrada";
    public string UsuarioResponsableId { get; set; } = string.Empty;

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
