using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/membresias")]
[Authorize(Policy = ApiConfiguracion.Recepcion)]
public sealed class MembresiasApiController(IMembresiaService membresias) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MembresiaDto>>> Listar(
        [StringLength(120)] string? busqueda,
        [RegularExpression("^(vigentes|porvencer|pendientes|vencidas|programadas|canceladas)$")] string? estado) =>
        await membresias.GetAllAsync(busqueda, estado);

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<MembresiaDto>> Obtener(int id) =>
        await membresias.GetByIdAsync(id) is { } membresia ? Ok(membresia) : NotFound();

    [HttpPost]
    [ProducesResponseType<MembresiaDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<MembresiaDto>> Crear(MembresiaEntrada entrada)
    {
        var membresia = await membresias.CreateAsync(new MembresiaCreateViewModel
        {
            SocioId = entrada.SocioId,
            TipoMembresiaId = entrada.TipoMembresiaId,
            FechaInicio = entrada.FechaInicio,
            PeriodosAdquiridos = entrada.PeriodosAdquiridos,
            CobrarAhora = entrada.CobrarAhora!.Value
        });
        return CreatedAtAction(nameof(Obtener), new { id = membresia.Id }, membresia);
    }

    [HttpPost("{id:int:min(1)}/cancelacion")]
    public async Task<ActionResult<ResultadoCancelacion>> Cancelar(int id) =>
        await membresias.CancelarAsync(id) is { } resultado ? Ok(resultado) : NotFound();
}
