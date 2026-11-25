using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/compras")]
[Authorize(Policy = ApiConfiguracion.Inventario)]
public sealed class ComprasApiController(ICompraService compras) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ComprasRespuesta>> Listar([FromQuery] PeriodoApi periodo,
        [Range(1, int.MaxValue)] int? proveedorId, [RegularExpression("^(Registrada|Anulada)$")] string? estado)
    {
        var lista = await compras.ObtenerListadoAsync(new CompraListaViewModel
        {
            FechaDesde = periodo.FechaDesde,
            FechaHasta = periodo.FechaHasta,
            ProveedorId = proveedorId,
            Estado = estado
        });
        return new ComprasRespuesta(lista.Compras, lista.ResumenMes);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<CompraDto>> Obtener(int id) =>
        await compras.GetByIdAsync(id) is { } compra ? Ok(compra) : NotFound();

    [HttpPost]
    [ProducesResponseType<CompraDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CompraDto>> Registrar(CompraEntrada entrada)
    {
        var compra = await compras.RegistrarAsync(new CompraNuevaViewModel
        {
            ProveedorId = entrada.ProveedorId,
            Lineas = entrada.Lineas!.Select(linea => new LineaCompraViewModel
            {
                ProductoId = linea!.ProductoId,
                Cantidad = linea.Cantidad,
                CostoUnitario = linea.CostoUnitario
            }).ToList()
        });
        return CreatedAtAction(nameof(Obtener), new { id = compra.Id }, compra);
    }

    [HttpPost("{id:int:min(1)}/anulacion")]
    public async Task<IActionResult> Anular(int id) =>
        SinContenidoSiExiste(await compras.AnularAsync(id));
}
