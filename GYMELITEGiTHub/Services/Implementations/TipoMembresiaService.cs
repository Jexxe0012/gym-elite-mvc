using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Models;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;

namespace GYMELITEGiTHub.Services.Implementations;

public class TipoMembresiaService : ITipoMembresiaService
{
    private readonly IRepository<TipoMembresia> _tipos;
    private readonly IUnitOfWork _unitOfWork;

    public TipoMembresiaService(IRepository<TipoMembresia> tipos, IUnitOfWork unitOfWork)
    {
        _tipos = tipos;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TipoMembresiaDto>> GetAllAsync(bool soloActivos = false)
    {
        var tipos = soloActivos
            ? await _tipos.FindAsync(t => t.Activo)
            : await _tipos.GetAllAsync();

        return tipos.OrderBy(t => t.Precio).Select(ADto).ToList();
    }

    public async Task<TipoMembresiaDto?> GetByIdAsync(int id)
    {
        var tipo = await _tipos.GetByIdAsync(id);
        return tipo is null ? null : ADto(tipo);
    }

    public async Task<TipoMembresiaDto> CreateAsync(TipoMembresiaFormViewModel modelo)
    {
        await ValidarReglasAsync(modelo);

        var tipo = new TipoMembresia
        {
            Nombre = modelo.Nombre.Trim(),
            Modalidad = modelo.Modalidad,
            DuracionDias = modelo.DuracionDias,
            Precio = modelo.Precio,
            Activo = true
        };

        await _tipos.AddAsync(tipo);
        await _unitOfWork.SaveChangesAsync();
        return ADto(tipo);
    }

    public async Task<bool> UpdateAsync(TipoMembresiaFormViewModel modelo)
    {
        var tipo = await _tipos.GetByIdAsync(modelo.Id);
        if (tipo is null) return false;

        await ValidarReglasAsync(modelo);

        tipo.Nombre = modelo.Nombre.Trim();
        tipo.Modalidad = modelo.Modalidad;
        tipo.DuracionDias = modelo.DuracionDias;
        tipo.Precio = modelo.Precio;
        tipo.Activo = modelo.Activo;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<TipoMembresiaDto?> CambiarEstadoAsync(int id)
    {
        var tipo = await _tipos.GetByIdAsync(id);
        if (tipo is null) return null;

        tipo.Activo = !tipo.Activo;
        await _unitOfWork.SaveChangesAsync();
        return ADto(tipo);
    }

    private async Task ValidarReglasAsync(TipoMembresiaFormViewModel modelo)
    {
        var nombre = modelo.Nombre.Trim();

        if (await _tipos.AnyAsync(t => t.Nombre == nombre && t.Id != modelo.Id))
            throw new InvalidOperationException("Ya existe un plan con ese nombre.");

        if (modelo.Modalidad == "Pase diario" && modelo.DuracionDias != 1)
            throw new InvalidOperationException("Un pase diario debe durar exactamente 1 día.");
    }

    private static TipoMembresiaDto ADto(TipoMembresia t) => new()
    {
        Id = t.Id,
        Nombre = t.Nombre,
        Modalidad = t.Modalidad,
        DuracionDias = t.DuracionDias,
        Precio = t.Precio,
        Activo = t.Activo
    };
}
