using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/planes")]
[Authorize(Policy = ApiConfiguracion.Recepcion)]
public sealed class PlanesApiController(ITipoMembresiaService planes) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TipoMembresiaDto>>> Listar(bool soloActivos = false) =>
        await planes.GetAllAsync(soloActivos);

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<TipoMembresiaDto>> Obtener(int id) =>
        await planes.GetByIdAsync(id) is { } plan ? Ok(plan) : NotFound();

    [HttpPost]
    [ProducesResponseType<TipoMembresiaDto>(StatusCodes.Status201Created)]
    [Authorize(Policy = ApiConfiguracion.Recepcion)]
    public async Task<ActionResult<TipoMembresiaDto>> Crear(PlanEntrada entrada)
    {
        var plan = await planes.CreateAsync(entrada.AModelo(0, true));
        return CreatedAtAction(nameof(Obtener), new { id = plan.Id }, plan);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = ApiConfiguracion.Recepcion)]
    public async Task<IActionResult> Actualizar(int id, PlanActualizacion entrada) =>
        SinContenidoSiExiste(await planes.UpdateAsync(entrada.AModelo(id, entrada.Activo!.Value)));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = ApiConfiguracion.Recepcion)]
    public async Task<IActionResult> Desactivar(int id) =>
        SinContenidoSiExiste(await planes.DesactivarAsync(id));
}
