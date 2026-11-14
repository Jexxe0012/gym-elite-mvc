using GYMELITEGiTHub.Dtos;

namespace GYMELITEGiTHub.Repositories.Interfaces;

public interface IReporteRepository
{
    Task<ResumenReporteDto> ObtenerResumenAsync(DateOnly desde, DateOnly hasta);
    Task<List<IngresoDiaDto>> ObtenerIngresosPorDiaAsync(DateOnly desde, DateOnly hasta);
    Task<List<IngresoModalidadDto>> ObtenerIngresosPorModalidadAsync(DateOnly desde, DateOnly hasta);
    Task<List<ProductoVendidoDto>> ObtenerProductosMasVendidosAsync(DateOnly desde, DateOnly hasta);
    Task<List<MovimientoProductoDto>> ObtenerMovimientosPorProductoAsync(DateOnly desde, DateOnly hasta);
    Task<List<ExistenciaCriticaDto>> ObtenerExistenciasCriticasAsync();
}
