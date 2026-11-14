using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

public class ReporteService : IReporteService
{
    private readonly IReporteRepository _reportes;

    public ReporteService(IReporteRepository reportes) => _reportes = reportes;

    public async Task<ReporteViewModel> GenerarAsync(ReporteViewModel modelo)
    {
        modelo.ReporteGenerado = false;

        if (!modelo.FechaDesde.HasValue || !modelo.FechaHasta.HasValue)
            throw new ValidationException("Seleccione las fechas inicial y final del reporte.");

        var desde = modelo.FechaDesde.Value;
        var hasta = modelo.FechaHasta.Value;
        if (ReporteViewModel.ErrorPeriodo(desde, hasta) is string error)
            throw new ValidationException(error);

        // El contexto es compartido por la solicitud: las consultas se esperan una a una.
        modelo.Resumen = await _reportes.ObtenerResumenAsync(desde, hasta);
        var ingresos = await _reportes.ObtenerIngresosPorDiaAsync(desde, hasta);
        modelo.IngresosPorDia = CompletarDias(desde, hasta, ingresos);
        modelo.IngresosPorModalidad = await _reportes.ObtenerIngresosPorModalidadAsync(desde, hasta);
        modelo.ProductosMasVendidos = await _reportes.ObtenerProductosMasVendidosAsync(desde, hasta);
        modelo.MovimientosPorProducto = await _reportes.ObtenerMovimientosPorProductoAsync(desde, hasta);
        modelo.ExistenciasCriticas = await _reportes.ObtenerExistenciasCriticasAsync();
        modelo.ReporteGenerado = true;
        return modelo;
    }

    private static List<IngresoDiaDto> CompletarDias(DateOnly desde, DateOnly hasta, List<IngresoDiaDto> ingresos)
    {
        var porFecha = ingresos.GroupBy(i => i.Fecha)
            .ToDictionary(g => g.Key, g => new IngresoDiaDto(
                g.Key, g.Sum(i => i.Membresias), g.Sum(i => i.Productos)));

        return Enumerable.Range(0, hasta.DayNumber - desde.DayNumber + 1)
            .Select(numero => desde.AddDays(numero))
            .Select(fecha => porFecha.TryGetValue(fecha, out var ingreso)
                ? ingreso : new IngresoDiaDto(fecha, 0m, 0m))
            .ToList();
    }
}
