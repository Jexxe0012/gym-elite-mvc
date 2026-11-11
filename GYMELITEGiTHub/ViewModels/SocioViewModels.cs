using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;

namespace GYMELITEGiTHub.ViewModels;

public class SocioCreateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "Teléfono no válido.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [Display(Name = "Correo")]
    public string? Correo { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }
}

public class SocioEditViewModel : SocioCreateViewModel
{
    public int Id { get; set; }

    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Display(Name = "Ficha habilitada")]
    public bool EstaHabilitado { get; set; } = true;
}

public class SocioListaViewModel
{
    public string? Busqueda { get; set; }
    public List<SocioDto> Socios { get; set; } = new();
}
