using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/socios")]
[Authorize(Policy = ApiConfiguracion.Recepcion)]
public sealed class SociosApiController(ISocioService socios, IMembresiaService membresias) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SocioDto>>> Listar([StringLength(100)] string? busqueda) =>
        await socios.GetAllAsync(busqueda);

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<SocioDto>> Obtener(int id) =>
        await socios.GetByIdAsync(id) is { } socio ? Ok(socio) : NotFound();

    [HttpGet("{id:int:min(1)}/membresias")]
    public async Task<ActionResult<List<MembresiaDto>>> ObtenerMembresias(int id)
    {
        if (await socios.GetByIdAsync(id) is null) return NotFound();
        return await membresias.GetPorSocioAsync(id);
    }

    [HttpPost]
    [ProducesResponseType<SocioDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SocioDto>> Crear(SocioCreateViewModel entrada)
    {
        var socio = await socios.CreateAsync(entrada);
        return CreatedAtAction(nameof(Obtener), new { id = socio.Id }, socio);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(int id, SocioActualizacion entrada) =>
        SinContenidoSiExiste(await socios.UpdateAsync(new SocioEditViewModel
        {
            Id = id,
            NombreCompleto = entrada.NombreCompleto,
            Telefono = entrada.Telefono,
            Correo = entrada.Correo,
            FechaNacimiento = entrada.FechaNacimiento,
            EstaHabilitado = entrada.EstaHabilitado!.Value
        }));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id) =>
        SinContenidoSiExiste(await socios.DesactivarAsync(id));
}
