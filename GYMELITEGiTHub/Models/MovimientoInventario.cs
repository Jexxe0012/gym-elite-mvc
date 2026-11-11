namespace GYMELITEGiTHub.Models;

public class MovimientoInventario
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string UsuarioResponsableId { get; set; } = string.Empty;
    public string OperacionOrigen { get; set; } = string.Empty;
    public int? OperacionOrigenId { get; set; }
}
