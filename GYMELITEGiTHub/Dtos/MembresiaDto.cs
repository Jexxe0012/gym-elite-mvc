using System.Globalization;
using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Dtos;

public enum EstadoVigencia
{
    Programada,
    Vigente,
    PorVencer,
    Vencida,
    PendientePago,
    Cancelada
}

/// <summary>Membresía con los datos del socio, del plan y del pago, lista para mostrar.</summary>
public class MembresiaDto
{
    public int Id { get; set; }
    public int SocioId { get; set; }
    public string SocioCodigo { get; set; } = string.Empty;
    public string SocioNombre { get; set; } = string.Empty;
    public int TipoMembresiaId { get; set; }
    public string TipoNombre { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int PeriodosAdquiridos { get; set; }

    /// <summary>Estado guardado: Registrada o Cancelada (los datos antiguos pueden traer "Vigente").</summary>
    public string Estado { get; set; } = string.Empty;

    // Pago válido (estado "Registrado"), si existe
    public bool TienePagoValido { get; set; }
    public int? PagoId { get; set; }
    public string? NumeroRecibo { get; set; }
    public decimal? MontoPagado { get; set; }
    public DateTime? FechaPago { get; set; }

    /// <summary>Monto a cobrar según el precio actual del plan. El monto cobrado realmente vive en Pago.</summary>
    public decimal Total => PrecioUnitario * PeriodosAdquiridos;
    public string TotalTexto => "$" + Total.ToString("N2", CultureInfo.InvariantCulture);
    public string MontoPagadoTexto =>
        MontoPagado is null ? string.Empty : "$" + MontoPagado.Value.ToString("N2", CultureInfo.InvariantCulture);

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    public int DiasRestantes => Math.Max(0, FechaFin.DayNumber - Hoy.DayNumber);

    public bool EstaCancelada => Estado == EstadosNegocio.MembresiaCancelada;

    /// <summary>
    /// Estado que se muestra. Se calcula con la cancelación, el pago y las fechas;
    /// no se guarda en la base de datos. Solo Vigente y PorVencer dan acceso.
    /// </summary>
    public EstadoVigencia Vigencia
    {
        get
        {
            if (EstaCancelada) return EstadoVigencia.Cancelada;

            var hoy = Hoy;
            if (!TienePagoValido)
                return hoy > FechaFin ? EstadoVigencia.Vencida : EstadoVigencia.PendientePago;

            if (hoy < FechaInicio) return EstadoVigencia.Programada;
            if (hoy > FechaFin) return EstadoVigencia.Vencida;
            return FechaFin.DayNumber - hoy.DayNumber <= 7
                ? EstadoVigencia.PorVencer
                : EstadoVigencia.Vigente;
        }
    }

    public bool EsEfectiva => Vigencia is EstadoVigencia.Vigente or EstadoVigencia.PorVencer;
}

/// <summary>Resultado de cancelar una membresía. ReciboAnulado trae el número si se anuló un pago.</summary>
public record ResultadoCancelacion(MembresiaDto Membresia, string? ReciboAnulado);
