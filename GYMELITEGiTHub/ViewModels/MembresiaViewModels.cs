using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.ViewModels;

/// <summary>Sirve para registrar una membresía nueva y también para renovar una existente.</summary>
public class MembresiaCreateViewModel
{
    [Required(ErrorMessage = "Seleccione un socio.")]
    [Display(Name = "Socio")]
    public int? SocioId { get; set; }

    [Required(ErrorMessage = "Seleccione un plan.")]
    [Display(Name = "Plan")]
    public int? TipoMembresiaId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateOnly? FechaInicio { get; set; }

    [Range(1, 36, ErrorMessage = "Los períodos deben estar entre 1 y 36.")]
    [Display(Name = "Períodos adquiridos")]
    public int PeriodosAdquiridos { get; set; } = 1;

    /// <summary>Si está marcada, se registra el pago en efectivo junto con la membresía.</summary>
    [Display(Name = "Cobrar ahora (efectivo)")]
    public bool CobrarAhora { get; set; } = true;

    // Listas para los desplegables: las llena el servicio, no vienen del formulario.
    [ValidateNever] public List<SelectListItem> Socios { get; set; } = new();
    [ValidateNever] public List<SelectListItem> Tipos { get; set; } = new();
}

public class MembresiaListaViewModel
{
    public string? Busqueda { get; set; }
    /// <summary>vigentes, porvencer, pendientes, vencidas, programadas, canceladas o vacío para todas.</summary>
    public string? Estado { get; set; }
    public List<MembresiaDto> Membresias { get; set; } = new();
}
