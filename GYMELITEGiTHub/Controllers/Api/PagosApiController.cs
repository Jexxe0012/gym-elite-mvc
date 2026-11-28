using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/pagos")]
[Authorize(Policy = ApiConfiguracion.Recepcion)]
public sealed class PagosApiController(IPagoService pagos) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagosRespuesta>> Listar([FromQuery] PeriodoApi periodo,
        [StringLength(120)] string? busqueda, [RegularExpression("^(Registrado|Anulado)$")] string? estado)
    {
        var lista = await pagos.ObtenerListadoAsync(new PagoListaViewModel
        {
            FechaDesde = periodo.FechaDesde,
            FechaHasta = periodo.FechaHasta,
            Busqueda = busqueda,
            Estado = estado
        });
        return new PagosRespuesta(lista.Pagos, lista.PagosHoy, lista.MontoHoy);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<PagoDto>> Obtener(int id) =>
        await pagos.GetByIdAsync(id) is { } pago ? Ok(pago) : NotFound();

    [HttpPost]
    [ProducesResponseType<PagoDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<PagoDto>> Registrar(CobroEntrada entrada)
    {
        var pago = await pagos.RegistrarAsync(new PagoRegistrarViewModel { MembresiaId = entrada.MembresiaId });
        return CreatedAtAction(nameof(Obtener), new { id = pago.Id }, pago);
    }

    [HttpPost("{id:int:min(1)}/anulacion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Anular(int id) =>
        SinContenidoSiExiste(await pagos.AnularAsync(id));
}
