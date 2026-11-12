using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GYMELITEGiTHub.ViewModels;

public class ReporteViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Seleccione la fecha inicial.")]
    [DataType(DataType.Date), Display(Name = "Desde")]
    public DateOnly? FechaDesde { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [Required(ErrorMessage = "Seleccione la fecha final.")]
    [DataType(DataType.Date), Display(Name = "Hasta")]
    public DateOnly? FechaHasta { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [ValidateNever] public bool ReporteGenerado { get; set; }
    [ValidateNever] public ResumenReporteDto Resumen { get; set; } = new(0m, 0m, 0, 0);
    [ValidateNever] public List<IngresoDiaDto> IngresosPorDia { get; set; } = new();
    [ValidateNever] public List<IngresoModalidadDto> IngresosPorModalidad { get; set; } = new();
    [ValidateNever] public List<ProductoVendidoDto> ProductosMasVendidos { get; set; } = new();
    [ValidateNever] public List<MovimientoProductoDto> MovimientosPorProducto { get; set; } = new();
    [ValidateNever] public List<ExistenciaCriticaDto> ExistenciasCriticas { get; set; } = new();

    public static string? ErrorPeriodo(DateOnly desde, DateOnly hasta)
    {
        if (desde > hasta)
            return "La fecha inicial no puede ser posterior a la final.";

        // Cerca del último año admitido, todo intervalo posible ya es menor de tres meses.
        if (desde <= DateOnly.MaxValue.AddMonths(-3) && hasta > desde.AddMonths(3))
            return "El período del reporte no puede superar tres meses.";

        return null;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaDesde.HasValue && FechaHasta.HasValue
            && ErrorPeriodo(FechaDesde.Value, FechaHasta.Value) is string error)
            yield return new ValidationResult(error, new[] { nameof(FechaHasta) });
    }
}
