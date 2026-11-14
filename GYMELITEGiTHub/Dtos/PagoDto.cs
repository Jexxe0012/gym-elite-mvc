using System.Globalization;
using GYMELITEGiTHub.Models;

namespace GYMELITEGiTHub.Dtos;

public class PagoDto
{
    public int Id { get; set; }
    public int MembresiaId { get; set; }
    public int SocioId { get; set; }
    public string SocioCodigo { get; set; } = string.Empty;
    public string SocioNombre { get; set; } = string.Empty;
    public string PlanNombre { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public int PeriodosAdquiridos { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public DateTime FechaHoraCobro { get; set; }
    public decimal Monto { get; set; }
    public string NumeroRecibo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? UsuarioResponsableId { get; set; }

    public bool EstaAnulado => Estado == EstadosNegocio.PagoAnulado;
    public string MontoTexto => "$" + Monto.ToString("N2", CultureInfo.InvariantCulture);
}

public record ResumenPagosDiaDto(int Cantidad, decimal Monto);
