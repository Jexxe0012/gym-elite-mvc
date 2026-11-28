using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/ventas")]
[Authorize(Policy = ApiConfiguracion.Inventario)]
public sealed class VentasApiController(IVentaService ventas) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VentaDto>>> Listar([FromQuery] PeriodoApi periodo) =>
        await ventas.GetAllAsync(periodo.FechaDesde?.ToDateTime(TimeOnly.MinValue),
            periodo.FechaHasta?.ToDateTime(TimeOnly.MinValue));

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<VentaDto>> Obtener(int id) =>
        await ventas.GetByIdAsync(id) is { } venta ? Ok(venta) : NotFound();

    [HttpPost]
    [ProducesResponseType<VentaDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<VentaDto>> Registrar(VentaEntrada entrada)
    {
        var venta = await ventas.RegistrarAsync(new VentaNuevaViewModel
        {
            Lineas = entrada.Lineas!.Select(linea => new VentaLineaViewModel
            {
                ProductoId = linea!.ProductoId,
                Cantidad = linea.Cantidad
            }).ToList()
        });
        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
    }

    [HttpPost("{id:int:min(1)}/anulacion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Anular(int id) =>
        SinContenidoSiExiste(await ventas.AnularAsync(id));
}
