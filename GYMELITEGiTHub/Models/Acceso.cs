namespace GYMELITEGiTHub.Models;

public class Acceso
{
    public int Id { get; set; }

    public int MembresiaId { get; set; }
    public Membresia? Membresia { get; set; }

    public DateTime FechaHoraIngreso { get; set; }
    public string? UsuarioResponsableId { get; set; }
}
