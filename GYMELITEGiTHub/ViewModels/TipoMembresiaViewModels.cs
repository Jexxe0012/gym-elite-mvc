using System.ComponentModel.DataAnnotations;

namespace GYMELITEGiTHub.ViewModels;

public class TipoMembresiaFormViewModel
{
    public static readonly string[] Modalidades = { "Mensualidad", "Pase diario" };

    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
    [Display(Name = "Nombre del plan")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione la modalidad.")]
    [Display(Name = "Modalidad")]
    public string Modalidad { get; set; } = string.Empty;

    [Range(1, 365, ErrorMessage = "La duración debe estar entre 1 y 365 días.")]
    [Display(Name = "Duración (días)")]
    public int DuracionDias { get; set; } = 30;

    [Range(typeof(decimal), "0.01", "99999", ErrorMessage = "El precio debe ser mayor que cero.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Display(Name = "Plan activo")]
    public bool Activo { get; set; } = true;
}
