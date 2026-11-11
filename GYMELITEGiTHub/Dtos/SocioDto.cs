namespace GYMELITEGiTHub.Dtos;

/// <summary>Datos de un socio que viajan entre servicio y controlador (sin exponer la entidad).</summary>
public class SocioDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public Guid CodigoQr { get; set; }
    public bool EstaHabilitado { get; set; }
}
