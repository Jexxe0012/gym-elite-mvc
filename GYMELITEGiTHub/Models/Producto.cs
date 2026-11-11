using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Models;

public class Producto
{
    public int Id { get; set; }

    [Required, StringLength(10)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Categoria { get; set; } = string.Empty;

    public decimal PrecioVenta { get; set; }
    public int Existencias { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}
