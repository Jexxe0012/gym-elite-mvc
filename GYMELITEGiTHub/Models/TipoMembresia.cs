using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Models;

public class TipoMembresia
{
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Modalidad { get; set; } = string.Empty;

    [Range(1, 365)]
    public int DuracionDias { get; set; }

    [Range(typeof(decimal), "0.01", "99999")]
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;
}
