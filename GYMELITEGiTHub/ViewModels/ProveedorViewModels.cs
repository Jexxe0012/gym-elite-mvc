using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;

namespace GYMELITEGiTHub.ViewModels;

public class ProveedorCreateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    [Display(Name = "Nombre del proveedor")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "Teléfono no válido.")]
    [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    [Display(Name = "Correo (opcional)")]
    public string? Correo { get; set; }
}

public class ProveedorEditViewModel : ProveedorCreateViewModel
{
    public int Id { get; set; }
}

public class ProveedorListaViewModel
{
    public string? Busqueda { get; set; }
    public List<ProveedorDto> Proveedores { get; set; } = new();
}
