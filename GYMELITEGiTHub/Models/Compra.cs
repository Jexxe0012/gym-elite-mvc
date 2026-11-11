namespace GYMELITEGiTHub.Models;

public class Compra
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "Registrada";
    public string UsuarioResponsableId { get; set; } = string.Empty;

    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
