using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Models;

public class Proveedor
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }

    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
