using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

/// <summary>
/// Reglas de negocio de Socios. No conoce el DbContext: solo habla con
/// el repositorio y la unidad de trabajo (inversión de dependencias).
/// </summary>
public class SocioService : ISocioService
{
    private readonly ISocioRepository _socios;
    private readonly IUnitOfWork _unitOfWork;

    public SocioService(ISocioRepository socios, IUnitOfWork unitOfWork)
    {
        _socios = socios;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SocioDto>> GetAllAsync(string? busqueda = null)
    {
        var socios = await _socios.BuscarAsync(busqueda);
        return socios.Select(ADto).ToList();
    }

    public async Task<SocioDto?> GetByIdAsync(int id)
    {
        var socio = await _socios.GetByIdAsync(id);
        return socio is null ? null : ADto(socio);
    }

    public async Task<SocioDto> CreateAsync(SocioCreateViewModel modelo)
    {
        var socio = new Socio
        {
            Codigo = await _socios.GenerarSiguienteCodigoAsync(),
            NombreCompleto = modelo.NombreCompleto.Trim(),
            Telefono = modelo.Telefono.Trim(),
            Correo = string.IsNullOrWhiteSpace(modelo.Correo) ? null : modelo.Correo.Trim(),
            FechaNacimiento = modelo.FechaNacimiento,
            CodigoQr = Guid.NewGuid(),
            EstaHabilitado = true
        };

        await _socios.AddAsync(socio);
        await _unitOfWork.SaveChangesAsync();
        return ADto(socio);
    }

    public async Task<bool> UpdateAsync(SocioEditViewModel modelo)
    {
        var socio = await _socios.GetByIdAsync(modelo.Id);
        if (socio is null) return false;

        socio.NombreCompleto = modelo.NombreCompleto.Trim();
        socio.Telefono = modelo.Telefono.Trim();
        socio.Correo = string.IsNullOrWhiteSpace(modelo.Correo) ? null : modelo.Correo.Trim();
        socio.FechaNacimiento = modelo.FechaNacimiento;
        socio.EstaHabilitado = modelo.EstaHabilitado;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var socio = await _socios.GetByIdAsync(id);
        if (socio is null) return false;

        socio.EstaHabilitado = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static SocioDto ADto(Socio s) => new()
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
}
