using System.Globalization;

namespace GYMELITEGiTHub.Dtos;

public class TipoMembresiaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public int DuracionDias { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }

    public string PrecioTexto => "$" + Precio.ToString("N2", CultureInfo.InvariantCulture);
}
