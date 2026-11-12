using System.Globalization;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GYMELITEGiTHub.Services.Implementations;

/// <summary>
/// Reglas de negocio de Membresías. No conoce el DbContext: habla con
/// repositorios y con la unidad de trabajo.
/// </summary>
public class MembresiaService : IMembresiaService
{
    private const string UsuarioSistema = "sistema"; // Hasta que exista autenticación (Fase 4)

    private readonly IMembresiaRepository _membresias;
    private readonly ISocioRepository _socios;
    private readonly IRepository<TipoMembresia> _tipos;
    private readonly IPagoRepository _pagos;
    private readonly IUnitOfWork _unitOfWork;

    public MembresiaService(
        IMembresiaRepository membresias,
        ISocioRepository socios,
        IRepository<TipoMembresia> tipos,
        IPagoRepository pagos,
        IUnitOfWork unitOfWork)
    {
        _membresias = membresias;
        _socios = socios;
        _tipos = tipos;
        _pagos = pagos;
        _unitOfWork = unitOfWork;
    }

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    public async Task<List<MembresiaDto>> GetAllAsync(string? busqueda = null, string? estado = null)
    {
        var lista = await _membresias.BuscarAsync(busqueda, estado, Hoy);
        return lista.Select(ADto).ToList();
    }

    public async Task<MembresiaDto?> GetByIdAsync(int id)
    {
        var membresia = await _membresias.GetConDetalleAsync(id);
        return membresia is null ? null : ADto(membresia);
    }

    public async Task<List<MembresiaDto>> GetPorSocioAsync(int socioId)
    {
        var lista = await _membresias.GetPorSocioAsync(socioId);
        return lista.Select(ADto).ToList();
    }

    public async Task<MembresiaDto?> GetVigenteDeSocioAsync(int socioId, DateOnly? fecha = null)
    {
        var membresia = await _membresias.GetVigenteAsync(socioId, fecha ?? Hoy);
        return membresia is null ? null : ADto(membresia);
    }

    public async Task<MembresiaDto> CreateAsync(MembresiaCreateViewModel modelo)
    {
        var socio = await _socios.GetByIdAsync(modelo.SocioId!.Value)
            ?? throw new InvalidOperationException("El socio seleccionado no existe.");

        if (!socio.EstaHabilitado)
            throw new InvalidOperationException("La ficha del socio está desactivada.");

        var tipo = await _tipos.GetByIdAsync(modelo.TipoMembresiaId!.Value)
            ?? throw new InvalidOperationException("El plan seleccionado no existe.");

        if (!tipo.Activo)
            throw new InvalidOperationException("El plan seleccionado no está activo.");

        var inicio = modelo.FechaInicio!.Value;
        // La fecha final es inclusiva: un plan de 30 días que inicia el 1 vence el 30.
        var fin = inicio.AddDays(tipo.DuracionDias * modelo.PeriodosAdquiridos - 1);

        if (await _membresias.ExisteTraslapeAsync(socio.Id, inicio, fin))
            throw new InvalidOperationException(
                "El socio ya tiene una membresía que cubre ese período. " +
                "Cancele la anterior o renueve a partir de la fecha siguiente a su vencimiento.");

        var membresia = new Membresia
        {
            SocioId = socio.Id,
            Socio = socio,
            TipoMembresiaId = tipo.Id,
            TipoMembresia = tipo,
            FechaInicio = inicio,
            FechaFin = fin,
            PeriodosAdquiridos = modelo.PeriodosAdquiridos,
            Estado = EstadosNegocio.MembresiaRegistrada
        };

        // Cobrar ahora: el pago se agrega a la misma membresía y se guarda en una sola operación
        // (SaveChanges es transaccional), así que no puede quedar una sin la otra.
        if (modelo.CobrarAhora)
        {
            membresia.Pagos.Add(new Pago
            {
                FechaHoraCobro = DateTime.Now,
                Monto = tipo.Precio * modelo.PeriodosAdquiridos,
                NumeroRecibo = await _pagos.GenerarSiguienteNumeroReciboAsync(),
                Estado = EstadosNegocio.PagoRegistrado,
                UsuarioResponsableId = UsuarioSistema
            });
        }

        await _membresias.AddAsync(membresia);
        await _unitOfWork.SaveChangesAsync();
        return ADto(membresia);
    }

    public async Task<ResultadoCancelacion?> CancelarAsync(int id)
    {
        var membresia = await _membresias.GetParaCancelarAsync(id);
        if (membresia is null) return null;

        if (membresia.Estado == EstadosNegocio.MembresiaCancelada)
            throw new InvalidOperationException("La membresía ya está cancelada.");

        if (membresia.Accesos.Any())
            throw new InvalidOperationException(
                "La membresía ya tiene ingresos registrados y no puede cancelarse.");

        // El pago anulado se conserva para el historial; no se borra nada.
        string? reciboAnulado = null;
        var pagoValido = membresia.Pagos.FirstOrDefault(p => p.Estado == EstadosNegocio.PagoRegistrado);
        if (pagoValido is not null)
        {
            pagoValido.Estado = EstadosNegocio.PagoAnulado;
            reciboAnulado = pagoValido.NumeroRecibo;
        }

        membresia.Estado = EstadosNegocio.MembresiaCancelada;
        await _unitOfWork.SaveChangesAsync();
        return new ResultadoCancelacion(ADto(membresia), reciboAnulado);
    }

    public async Task<MembresiaCreateViewModel> PrepararNuevaAsync()
    {
        var modelo = new MembresiaCreateViewModel
        {
            FechaInicio = Hoy,
            PeriodosAdquiridos = 1,
            CobrarAhora = true
        };
        await CargarOpcionesAsync(modelo);
        return modelo;
    }

    public async Task<MembresiaCreateViewModel?> PrepararRenovacionAsync(int socioId)
    {
        var socio = await _socios.GetByIdAsync(socioId);
        if (socio is null) return null;

        if (!socio.EstaHabilitado)
            throw new InvalidOperationException("La ficha del socio está desactivada.");

        var ultima = await _membresias.GetUltimaDeSocioAsync(socioId);
        var hoy = Hoy;

        var modelo = new MembresiaCreateViewModel
        {
            SocioId = socioId,
            TipoMembresiaId = ultima?.TipoMembresiaId,
            PeriodosAdquiridos = 1,
            CobrarAhora = true,
            // Si aún está cubierto, la renovación empieza el día siguiente al vencimiento.
            FechaInicio = ultima is not null && ultima.FechaFin >= hoy
                ? ultima.FechaFin.AddDays(1)
                : hoy
        };
        await CargarOpcionesAsync(modelo);
        return modelo;
    }

    public async Task CargarOpcionesAsync(MembresiaCreateViewModel modelo)
    {
        var socios = await _socios.BuscarAsync(null);
        modelo.Socios = socios
            .Where(s => s.EstaHabilitado)
            .OrderBy(s => s.NombreCompleto)
            .Select(s => new SelectListItem($"{s.Codigo} · {s.NombreCompleto}", s.Id.ToString()))
            .ToList();

        var tipos = await _tipos.FindAsync(t => t.Activo);
        modelo.Tipos = tipos
            .OrderBy(t => t.Precio)
            .Select(t => new SelectListItem(
                $"{t.Nombre} · {t.DuracionDias} día(s) · ${t.Precio.ToString("N2", CultureInfo.InvariantCulture)}",
                t.Id.ToString()))
            .ToList();
    }

    private static MembresiaDto ADto(Membresia m)
    {
        var pago = m.Pagos
            .Where(p => p.Estado == EstadosNegocio.PagoRegistrado)
            .OrderByDescending(p => p.FechaHoraCobro)
            .FirstOrDefault();

        return new MembresiaDto
        {
            Id = m.Id,
            SocioId = m.SocioId,
            SocioCodigo = m.Socio?.Codigo ?? string.Empty,
            SocioNombre = m.Socio?.NombreCompleto ?? string.Empty,
            TipoMembresiaId = m.TipoMembresiaId,
            TipoNombre = m.TipoMembresia?.Nombre ?? string.Empty,
            Modalidad = m.TipoMembresia?.Modalidad ?? string.Empty,
            PrecioUnitario = m.TipoMembresia?.Precio ?? 0m,
            FechaInicio = m.FechaInicio,
            FechaFin = m.FechaFin,
            PeriodosAdquiridos = m.PeriodosAdquiridos,
            Estado = m.Estado,
            TienePagoValido = pago is not null,
            PagoId = pago?.Id,
            NumeroRecibo = pago?.NumeroRecibo,
            MontoPagado = pago?.Monto,
            FechaPago = pago?.FechaHoraCobro
        };
    }
}
