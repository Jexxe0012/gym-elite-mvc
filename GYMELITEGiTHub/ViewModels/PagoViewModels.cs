using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.ViewModels;

public class PagoListaViewModel : IValidatableObject
{
    public string? Busqueda { get; set; }

    [RegularExpression("^(Registrado|Anulado)$", ErrorMessage = "Seleccione un estado de pago válido.")]
    public string? Estado { get; set; }

    [DataType(DataType.Date), Display(Name = "Desde")]
    public DateOnly? FechaDesde { get; set; }

    [DataType(DataType.Date), Display(Name = "Hasta")]
    public DateOnly? FechaHasta { get; set; }

    [ValidateNever] public List<PagoDto> Pagos { get; set; } = new();
    [ValidateNever] public int PagosHoy { get; set; }
    [ValidateNever] public decimal MontoHoy { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaDesde.HasValue && FechaHasta.HasValue && FechaDesde.Value > FechaHasta.Value)
            yield return new ValidationResult("La fecha inicial no puede ser posterior a la final.", new[] { nameof(FechaHasta) });
    }
}

public class PagoRegistrarViewModel
{
    [Required(ErrorMessage = "Seleccione una membresía pendiente de pago.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una membresía válida.")]
    [Display(Name = "Membresía")]
    public int? MembresiaId { get; set; }

    [ValidateNever] public List<SelectListItem> Membresias { get; set; } = new();
    [ValidateNever] public MembresiaDto? MembresiaSeleccionada { get; set; }
}
