using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.ViewModels;

public class CompraListaViewModel : IValidatableObject
{
    [Display(Name = "Proveedor")]
    public int? ProveedorId { get; set; }

    [RegularExpression("^(Registrada|Anulada)$", ErrorMessage = "Seleccione un estado de compra válido.")]
    public string? Estado { get; set; }

    [DataType(DataType.Date), Display(Name = "Desde")]
    public DateOnly? FechaDesde { get; set; }

    [DataType(DataType.Date), Display(Name = "Hasta")]
    public DateOnly? FechaHasta { get; set; }

    [ValidateNever] public List<CompraDto> Compras { get; set; } = new();
    [ValidateNever] public List<SelectListItem> Proveedores { get; set; } = new();
    [ValidateNever] public ResumenComprasDto ResumenMes { get; set; } = new(0, 0m);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaDesde.HasValue && FechaHasta.HasValue && FechaDesde.Value > FechaHasta.Value)
            yield return new ValidationResult("La fecha inicial no puede ser posterior a la final.", new[] { nameof(FechaHasta) });
    }
}

/// <summary>Una línea del formulario de compra. Las líneas totalmente vacías se ignoran.</summary>
public class LineaCompraViewModel
{
    [Display(Name = "Producto")]
    public int? ProductoId { get; set; }

    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    [Display(Name = "Cantidad")]
    public int? Cantidad { get; set; }

    [Range(typeof(decimal), "0.01", "999999", ErrorMessage = "El costo unitario debe ser mayor que cero.")]
    [Display(Name = "Costo unitario")]
    public decimal? CostoUnitario { get; set; }

    public bool EstaVacia => ProductoId is null && Cantidad is null && CostoUnitario is null;
}

public class CompraNuevaViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Seleccione un proveedor.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor válido.")]
    [Display(Name = "Proveedor")]
    public int? ProveedorId { get; set; }

    public List<LineaCompraViewModel> Lineas { get; set; } = new();

    [ValidateNever] public List<SelectListItem> Proveedores { get; set; } = new();
    [ValidateNever] public List<SelectListItem> Productos { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var lineas = Lineas.Where(l => !l.EstaVacia).ToList();

        if (lineas.Count == 0)
            yield return new ValidationResult("Agregue al menos una línea con producto, cantidad y costo.");

        for (var i = 0; i < Lineas.Count; i++)
        {
            var linea = Lineas[i];
            if (linea.EstaVacia) continue;

            if (linea.ProductoId is null or <= 0)
                yield return new ValidationResult($"Línea {i + 1}: seleccione un producto.", new[] { $"Lineas[{i}].ProductoId" });
            if (linea.Cantidad is null)
                yield return new ValidationResult($"Línea {i + 1}: indique la cantidad.", new[] { $"Lineas[{i}].Cantidad" });
            if (linea.CostoUnitario is null)
                yield return new ValidationResult($"Línea {i + 1}: indique el costo unitario.", new[] { $"Lineas[{i}].CostoUnitario" });
        }

        var repetidos = lineas.Where(l => l.ProductoId > 0)
            .GroupBy(l => l.ProductoId)
            .Any(g => g.Count() > 1);
        if (repetidos)
            yield return new ValidationResult("Un producto aparece en más de una línea. Combine las cantidades en una sola línea.");
    }
}
