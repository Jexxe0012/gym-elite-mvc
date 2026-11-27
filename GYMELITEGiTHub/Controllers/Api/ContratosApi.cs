using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Controllers.Api;

public sealed class SocioActualizacion : SocioCreateViewModel
{
    [Required]
    public bool? EstaHabilitado { get; init; }
}

public sealed record ProductoActualizacion(
    [Required, StringLength(120)] string Nombre,
    [Required, StringLength(40)] string Categoria,
    [Range(typeof(decimal), "0.01", "999999.99")] decimal PrecioVenta,
    [Required, Range(0, int.MaxValue)] int? StockMinimo,
    [Required] bool? Activo);

public sealed record AjusteInventarioEntrada(
    [Required, RegularExpression("^(Entrada|Salida)$")] string Tipo,
    [Range(1, int.MaxValue)] int Cantidad,
    [Required, StringLength(200)] string Motivo);

public sealed record ProductosRespuesta(
    List<ProductoDto> Productos, int ProductosActivos, int ProductosStockBajo, long UnidadesDisponibles);

public class PlanEntrada
{
    [Required, StringLength(60)]
    public string Nombre { get; init; } = string.Empty;

    [Required, RegularExpression("^(Mensualidad|Pase diario)$")]
    public string Modalidad { get; init; } = string.Empty;

    [Range(1, 365)]
    public int DuracionDias { get; init; }

    [Range(typeof(decimal), "0.01", "99999")]
    public decimal Precio { get; init; }

    internal TipoMembresiaFormViewModel AModelo(int id, bool activo) => new()
    {
        Id = id,
        Nombre = Nombre,
        Modalidad = Modalidad,
        DuracionDias = DuracionDias,
        Precio = Precio,
        Activo = activo
    };
}

public sealed class PlanActualizacion : PlanEntrada
{
    [Required]
    public bool? Activo { get; init; }
}

public sealed record MembresiaEntrada(
    [Range(1, int.MaxValue)] int SocioId,
    [Range(1, int.MaxValue)] int TipoMembresiaId,
    [Required] DateOnly? FechaInicio,
    [Range(1, 36)] int PeriodosAdquiridos,
    [Required] bool? CobrarAhora);

public sealed record IdentificacionSocio([Range(1, int.MaxValue)] int SocioId);
public sealed record CobroEntrada([Range(1, int.MaxValue)] int MembresiaId);
public sealed record PagosRespuesta(List<PagoDto> Pagos, int PagosHoy, decimal MontoHoy);
public sealed record AccesosResumenRespuesta(int IngresosHoy, int SociosHoy, List<AccesoDto> Recientes);
public sealed record ComprasRespuesta(List<CompraDto> Compras, ResumenComprasDto ResumenMes);

public sealed class PeriodoApi : IValidatableObject
{
    public DateOnly? FechaDesde { get; init; }
    public DateOnly? FechaHasta { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaDesde > FechaHasta)
            yield return new ValidationResult("La fecha inicial no puede ser posterior a la final.",
                new[] { nameof(FechaHasta) });
    }
}

public class LineaVentaEntrada
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; init; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; init; }
}

public sealed class LineaCompraEntrada : LineaVentaEntrada
{
    [Range(typeof(decimal), "0.01", "999999")]
    public decimal CostoUnitario { get; init; }
}

public abstract class OperacionInventarioEntrada<TLinea> : IValidatableObject where TLinea : LineaVentaEntrada
{
    [Required, MinLength(1), MaxLength(100)]
    public List<TLinea?>? Lineas { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Lineas is null) yield break;

        var productos = new HashSet<int>();
        for (var i = 0; i < Lineas.Count; i++)
        {
            if (Lineas[i] is not { } linea)
                yield return new ValidationResult("La línea es obligatoria.", new[] { $"Lineas[{i}]" });
            else if (!productos.Add(linea.ProductoId))
                yield return new ValidationResult("Combine en una sola línea las cantidades del mismo producto.",
                    new[] { $"Lineas[{i}].ProductoId" });
        }
    }
}

public sealed class VentaEntrada : OperacionInventarioEntrada<LineaVentaEntrada>;

public sealed class CompraEntrada : OperacionInventarioEntrada<LineaCompraEntrada>
{
    [Range(1, int.MaxValue)]
    public int ProveedorId { get; init; }
}
