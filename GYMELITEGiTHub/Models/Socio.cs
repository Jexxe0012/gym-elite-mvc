using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Models;

public class Socio
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, Phone]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress]
    public string? Correo { get; set; }

    public DateOnly? FechaNacimiento { get; set; }
    public Guid CodigoQr { get; set; } = Guid.NewGuid();
    public bool EstaHabilitado { get; set; } = true;

    public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();
}
