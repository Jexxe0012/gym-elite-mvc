using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;

namespace GYMELITEGiTHub.ViewModels;

/// <summary>Pantalla de recepción: búsqueda o QR, resultado de la verificación y últimos ingresos.</summary>
public class AccesoVerificarViewModel
{
    [StringLength(100, ErrorMessage = "La búsqueda no puede superar 100 caracteres.")]
    [Display(Name = "Código QR, código de socio, nombre o teléfono")]
    public string? Busqueda { get; set; }

    public VerificacionAccesoDto? Verificacion { get; set; }
    public List<AccesoDto> Recientes { get; set; } = new();
    public int IngresosHoy { get; set; }
    public int SociosHoy { get; set; }
}

public class AccesoHistorialViewModel
{
    public const int LimiteRegistros = 200;

    [StringLength(100, ErrorMessage = "La búsqueda no puede superar 100 caracteres.")]
    [Display(Name = "Buscar")]
    public string? Busqueda { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateOnly? FechaDesde { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateOnly? FechaHasta { get; set; }

    public List<AccesoDto> Accesos { get; set; } = new();
}
