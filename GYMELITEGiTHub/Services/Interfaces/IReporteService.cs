using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Interfaces;

public interface IReporteService
{
    Task<ReporteViewModel> GenerarAsync(ReporteViewModel modelo);
}
