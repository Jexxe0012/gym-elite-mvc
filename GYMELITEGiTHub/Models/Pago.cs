using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.Models;

public class Pago
{
    public int Id { get; set; }

    public int MembresiaId { get; set; }
    public Membresia? Membresia { get; set; }

    public DateTime FechaHoraCobro { get; set; }

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal Monto { get; set; }

    public string NumeroRecibo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Registrado";
    public string? UsuarioResponsableId { get; set; }
}
