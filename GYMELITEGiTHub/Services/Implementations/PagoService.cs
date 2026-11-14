using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.Services.Implementations;

public class PagoService : IPagoService
{
    private const string UsuarioSistema = "sistema";
    private readonly IPagoRepository _pagos;
    private readonly IMembresiaService _membresias;
    private readonly IUnitOfWork _unitOfWork;

    public PagoService(IPagoRepository pagos, IMembresiaService membresias, IUnitOfWork unitOfWork)
    {
        _pagos = pagos;
        _membresias = membresias;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagoListaViewModel> ObtenerListadoAsync(PagoListaViewModel filtros)
    {
        if (filtros.FechaDesde.HasValue && filtros.FechaHasta.HasValue &&
            filtros.FechaDesde.Value > filtros.FechaHasta.Value)
            throw new InvalidOperationException("La fecha inicial no puede ser posterior a la final.");

        if (!string.IsNullOrWhiteSpace(filtros.Estado) &&
            filtros.Estado != EstadosNegocio.PagoRegistrado && filtros.Estado != EstadosNegocio.PagoAnulado)
            throw new InvalidOperationException("Seleccione un estado de pago válido.");

        var pagos = await _pagos.BuscarAsync(filtros.Busqueda, filtros.Estado, filtros.FechaDesde, filtros.FechaHasta);
        var resumen = await _pagos.ObtenerResumenDelDiaAsync(DateOnly.FromDateTime(DateTime.Today));
        filtros.Pagos = pagos.Select(p => ADto(p)).ToList();
        filtros.PagosHoy = resumen.Cantidad;
        filtros.MontoHoy = resumen.Monto;
        return filtros;
    }

    public async Task<PagoDto?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        var pago = await _pagos.GetConDetalleAsync(id);
        return pago is null ? null : ADto(pago);
    }

    public async Task<PagoRegistrarViewModel> PrepararRegistroAsync(int? membresiaId = null)
    {
        var modelo = new PagoRegistrarViewModel { MembresiaId = membresiaId };
        await CargarOpcionesAsync(modelo);

        if (membresiaId.HasValue && modelo.MembresiaSeleccionada is null)
            throw new InvalidOperationException("La membresía seleccionada no está disponible para el cobro. Debe estar pendiente de pago.");

        return modelo;
    }

    public async Task CargarOpcionesAsync(PagoRegistrarViewModel modelo)
    {
        var pendientes = await _membresias.GetAllAsync(estado: "pendientes");
        pendientes = pendientes.Where(m => m.Vigencia == EstadoVigencia.PendientePago).ToList();
        modelo.Membresias = pendientes
            .OrderBy(m => m.SocioNombre)
            .ThenBy(m => m.FechaInicio)
            .Select(m => new SelectListItem(
                $"{m.SocioCodigo} · {m.SocioNombre} · {m.TipoNombre} · {m.TotalTexto}", m.Id.ToString()))
            .ToList();
        modelo.MembresiaSeleccionada = pendientes.FirstOrDefault(m => m.Id == modelo.MembresiaId);
    }

    public async Task<PagoDto> RegistrarAsync(PagoRegistrarViewModel modelo)
    {
        if (!modelo.MembresiaId.HasValue || modelo.MembresiaId.Value <= 0)
            throw new InvalidOperationException("Seleccione una membresía pendiente de pago.");

        var membresia = await _membresias.GetByIdAsync(modelo.MembresiaId.Value)
            ?? throw new InvalidOperationException("La membresía seleccionada no existe.");

        if (membresia.EstaCancelada)
            throw new InvalidOperationException("No se puede cobrar una membresía cancelada.");

        if (membresia.TienePagoValido || await _pagos.ExistePagoValidoAsync(membresia.Id))
            throw new InvalidOperationException("La membresía ya tiene un pago válido. Puede consultar o reimprimir su recibo.");

        if (membresia.Vigencia != EstadoVigencia.PendientePago)
            throw new InvalidOperationException("La membresía no está pendiente de pago. Revise sus fechas antes de cobrar.");

        if (membresia.Total <= 0m || membresia.Total > 999999m)
            throw new InvalidOperationException("El importe de la membresía debe ser mayor que cero y no superar $999,999.00.");

        var pago = new Pago
        {
            MembresiaId = membresia.Id,
            FechaHoraCobro = DateTime.Now,
            Monto = membresia.Total,
            NumeroRecibo = await _pagos.GenerarSiguienteNumeroReciboAsync(),
            Estado = EstadosNegocio.PagoRegistrado,
            UsuarioResponsableId = UsuarioSistema
        };

        await _pagos.AddAsync(pago);
        await _unitOfWork.SaveChangesAsync();
        return ADto(pago, membresia);
    }

    public async Task<bool> AnularAsync(int id)
    {
        if (id <= 0) return false;
        var pago = await _pagos.GetByIdAsync(id);
        if (pago is null) return false;

        if (pago.Estado == EstadosNegocio.PagoAnulado)
            throw new InvalidOperationException("El pago ya está anulado.");
        if (pago.Estado != EstadosNegocio.PagoRegistrado)
            throw new InvalidOperationException("Solo se puede anular un pago registrado.");

        pago.Estado = EstadosNegocio.PagoAnulado;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static PagoDto ADto(Pago pago, MembresiaDto? membresia = null) => new()
    {
        Id = pago.Id,
        MembresiaId = pago.MembresiaId,
        SocioId = membresia?.SocioId ?? pago.Membresia?.SocioId ?? 0,
        SocioCodigo = membresia?.SocioCodigo ?? pago.Membresia?.Socio?.Codigo ?? string.Empty,
        SocioNombre = membresia?.SocioNombre ?? pago.Membresia?.Socio?.NombreCompleto ?? string.Empty,
        PlanNombre = membresia?.TipoNombre ?? pago.Membresia?.TipoMembresia?.Nombre ?? string.Empty,
        Modalidad = membresia?.Modalidad ?? pago.Membresia?.TipoMembresia?.Modalidad ?? string.Empty,
        PeriodosAdquiridos = membresia?.PeriodosAdquiridos ?? pago.Membresia?.PeriodosAdquiridos ?? 0,
        FechaInicio = membresia?.FechaInicio ?? pago.Membresia?.FechaInicio ?? default,
        FechaFin = membresia?.FechaFin ?? pago.Membresia?.FechaFin ?? default,
        FechaHoraCobro = pago.FechaHoraCobro,
        Monto = pago.Monto,
        NumeroRecibo = pago.NumeroRecibo,
        Estado = pago.Estado,
        UsuarioResponsableId = pago.UsuarioResponsableId
    };
}
