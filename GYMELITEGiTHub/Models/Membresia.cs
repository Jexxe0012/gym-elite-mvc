using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Models;

public class Membresia
{
    public int Id { get; set; }

    public int SocioId { get; set; }
    public Socio? Socio { get; set; }

    public int TipoMembresiaId { get; set; }
    public TipoMembresia? TipoMembresia { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    [Range(1, 36)]
    public int PeriodosAdquiridos { get; set; } = 1;

    public string Estado { get; set; } = "Vigente";

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<Acceso> Accesos { get; set; } = new List<Acceso>();
}
