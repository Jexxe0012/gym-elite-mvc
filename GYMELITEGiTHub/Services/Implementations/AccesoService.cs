using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

/// <summary>
/// Reglas de negocio de Accesos. La cobertura (membresía no cancelada, con pago válido y dentro
/// de sus fechas) la decide MembresiaService; aquí solo se busca al socio y se registra el ingreso.
/// </summary>
public class AccesoService : IAccesoService
{
    private const string UsuarioSistema = "sistema"; // Hasta que exista autenticación (Fase 4)
    private const int MaximoCandidatos = 10;

    // Evita registrar dos veces el mismo ingreso por un doble clic o un doble escaneo.
    private static readonly TimeSpan VentanaDobleRegistro = TimeSpan.FromMinutes(2);

    private readonly IAccesoRepository _accesos;
    private readonly ISocioRepository _socios;
    private readonly IMembresiaService _membresias;
    private readonly IUnitOfWork _unitOfWork;

    public AccesoService(
        IAccesoRepository accesos,
        ISocioRepository socios,
        IMembresiaService membresias,
        IUnitOfWork unitOfWork)
    {
        _accesos = accesos;
        _socios = socios;
        _membresias = membresias;
        _unitOfWork = unitOfWork;
    }

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    public async Task PrepararAsync(AccesoVerificarViewModel modelo)
    {
        modelo.Recientes = await GetRecientesAsync();

        var resumen = await _accesos.ObtenerResumenDelDiaAsync(Hoy);
        modelo.IngresosHoy = resumen.Ingresos;
        modelo.SociosHoy = resumen.SociosDistintos;

        if (!string.IsNullOrWhiteSpace(modelo.Busqueda))
            modelo.Verificacion = await VerificarAsync(modelo.Busqueda);
    }

    public async Task<VerificacionAccesoDto> VerificarAsync(string? busqueda)
    {
        var termino = (busqueda ?? string.Empty).Trim();

        if (termino.Length == 0)
            throw new InvalidOperationException("Escanee un código QR o escriba un código, nombre o teléfono.");
        if (termino.Length > 100)
            throw new InvalidOperationException("La búsqueda no puede superar 100 caracteres.");

        var verificacion = new VerificacionAccesoDto { Busqueda = termino };
        var esQr = Guid.TryParse(termino, out var codigoQr);

        List<Socio> encontrados;
        if (esQr)
        {
            encontrados = await _socios.FindAsync(s => s.CodigoQr == codigoQr);
        }
        else
        {
            // Primero el código exacto (S-001); si no hay, búsqueda por nombre, código o teléfono.
            encontrados = await _socios.FindAsync(s => s.Codigo == termino);
            if (encontrados.Count == 0)
                encontrados = await _socios.BuscarAsync(termino);
        }

        if (encontrados.Count == 0)
        {
            verificacion.Resultado = ResultadoBusquedaAcceso.NoEncontrado;
            verificacion.Motivo = esQr
                ? "El código QR no corresponde a ningún socio. Si el QR fue repuesto, el anterior ya no es válido."
                : "No se encontró ningún socio con ese dato.";
            return verificacion;
        }

        if (encontrados.Count > 1)
        {
            verificacion.Resultado = ResultadoBusquedaAcceso.VariosResultados;
            verificacion.TotalCandidatos = encontrados.Count;
            verificacion.Candidatos = encontrados.Take(MaximoCandidatos).Select(ASocioDto).ToList();
            verificacion.Motivo = "Hay varios socios que coinciden. Elija uno para verificar su acceso.";
            return verificacion;
        }

        var socio = encontrados[0];
        verificacion.Resultado = ResultadoBusquedaAcceso.Encontrado;
        verificacion.Socio = ASocioDto(socio);
        verificacion.UltimoIngreso = await _accesos.GetUltimoIngresoDeSocioAsync(socio.Id);

        var (vigente, referencia, motivo) = await EvaluarCoberturaAsync(socio);
        verificacion.Autorizado = vigente is not null;
        verificacion.Membresia = vigente ?? referencia;
        verificacion.Motivo = motivo;
        return verificacion;
    }

    public async Task<AccesoDto> RegistrarIngresoAsync(int socioId)
    {
        var socio = await _socios.GetByIdAsync(socioId)
            ?? throw new InvalidOperationException("El socio seleccionado no existe.");

        // La cobertura se vuelve a calcular aquí: no se confía en lo que mostró la pantalla.
        var (vigente, _, motivo) = await EvaluarCoberturaAsync(socio);
        if (vigente is null)
            throw new InvalidOperationException($"No se registró el ingreso. {motivo}");

        var desde = DateTime.Now - VentanaDobleRegistro;
        if (await _accesos.ExisteIngresoRecienteAsync(vigente.Id, desde))
            throw new InvalidOperationException("El ingreso de este socio ya fue registrado hace un momento.");

        var acceso = new Acceso
        {
            MembresiaId = vigente.Id,
            FechaHoraIngreso = DateTime.Now,
            UsuarioResponsableId = UsuarioSistema
        };

        await _accesos.AddAsync(acceso);
        await _unitOfWork.SaveChangesAsync();

        return new AccesoDto
        {
            Id = acceso.Id,
            MembresiaId = vigente.Id,
            SocioId = socio.Id,
            SocioCodigo = socio.Codigo,
            SocioNombre = socio.NombreCompleto,
            PlanNombre = vigente.TipoNombre,
            FechaHoraIngreso = acceso.FechaHoraIngreso,
            UsuarioResponsableId = acceso.UsuarioResponsableId
        };
    }

    public async Task<List<AccesoDto>> GetRecientesAsync(int cantidad = 8)
    {
        var accesos = await _accesos.GetRecientesAsync(cantidad);
        return accesos.Select(ADto).ToList();
    }

    public async Task<AccesoHistorialViewModel> ObtenerHistorialAsync(AccesoHistorialViewModel filtros)
    {
        if (filtros.FechaDesde.HasValue && filtros.FechaHasta.HasValue &&
            filtros.FechaDesde.Value > filtros.FechaHasta.Value)
            throw new InvalidOperationException("La fecha inicial no puede ser posterior a la final.");

        var accesos = await _accesos.BuscarAsync(
            filtros.Busqueda, filtros.FechaDesde, filtros.FechaHasta, AccesoHistorialViewModel.LimiteRegistros);

        filtros.Accesos = accesos.Select(ADto).ToList();
        return filtros;
    }

    // ---------- Reglas internas ----------

    /// <summary>
    /// Devuelve la membresía que da acceso hoy (o null) y, si no hay, la membresía de referencia
    /// y el motivo del rechazo.
    /// </summary>
    private async Task<(MembresiaDto? Vigente, MembresiaDto? Referencia, string Motivo)> EvaluarCoberturaAsync(Socio socio)
    {
        if (!socio.EstaHabilitado)
            return (null, null, "La ficha del socio está desactivada.");

        var vigente = await _membresias.GetVigenteDeSocioAsync(socio.Id, Hoy);
        if (vigente is not null)
            return (vigente, vigente, string.Empty);

        // Sin cobertura: se toma la membresía no cancelada más reciente para explicar el motivo.
        var todas = await _membresias.GetPorSocioAsync(socio.Id);
        var referencia = todas.FirstOrDefault(m => !m.EstaCancelada) ?? todas.FirstOrDefault();
        return (null, referencia, MotivoDeRechazo(referencia));
    }

    private static string MotivoDeRechazo(MembresiaDto? membresia)
    {
        if (membresia is null)
            return "El socio no tiene membresías registradas.";

        return membresia.Vigencia switch
        {
            EstadoVigencia.Cancelada     => "La membresía fue cancelada.",
            EstadoVigencia.PendientePago => "La membresía está pendiente de pago.",
            EstadoVigencia.Programada    => $"La membresía inicia el {membresia.FechaInicio:dd/MM/yyyy}.",
            EstadoVigencia.Vencida when !membresia.TienePagoValido
                                         => $"La membresía venció el {membresia.FechaFin:dd/MM/yyyy} y no tiene pago registrado.",
            EstadoVigencia.Vencida       => $"La membresía venció el {membresia.FechaFin:dd/MM/yyyy}.",
            _                            => "El socio no tiene una membresía vigente."
        };
    }

    private static SocioDto ASocioDto(Socio s) => new()
    {
        Id = s.Id,
        Codigo = s.Codigo,
        NombreCompleto = s.NombreCompleto,
        Telefono = s.Telefono,
        Correo = s.Correo,
        FechaNacimiento = s.FechaNacimiento,
        CodigoQr = s.CodigoQr,
        EstaHabilitado = s.EstaHabilitado
    };

    private static AccesoDto ADto(Acceso a) => new()
    {
        Id = a.Id,
        MembresiaId = a.MembresiaId,
        SocioId = a.Membresia?.SocioId ?? 0,
        SocioCodigo = a.Membresia?.Socio?.Codigo ?? string.Empty,
        SocioNombre = a.Membresia?.Socio?.NombreCompleto ?? string.Empty,
        PlanNombre = a.Membresia?.TipoMembresia?.Nombre ?? string.Empty,
        FechaHoraIngreso = a.FechaHoraIngreso,
        UsuarioResponsableId = a.UsuarioResponsableId
    };
}
