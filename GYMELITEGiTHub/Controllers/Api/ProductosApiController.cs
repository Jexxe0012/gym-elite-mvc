using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/productos")]
[Authorize(Policy = ApiConfiguracion.Inventario)]
public sealed class ProductosApiController(IProductoService productos) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProductosRespuesta>> Listar(
        [StringLength(120)] string? busqueda, [StringLength(40)] string? categoria)
    {
        var lista = await productos.GetListaAsync(busqueda, categoria);
        return new ProductosRespuesta(lista.Productos, lista.ProductosActivos, lista.ProductosStockBajo,
            lista.UnidadesDisponibles);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<ProductoDto>> Obtener(int id) =>
        await productos.GetByIdAsync(id) is { } producto ? Ok(producto) : NotFound();

    [HttpPost]
    [ProducesResponseType<ProductoDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductoDto>> Crear(ProductoCreateViewModel entrada)
    {
        var producto = await productos.CreateAsync(entrada);
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(int id, ProductoActualizacion entrada) =>
        SinContenidoSiExiste(await productos.UpdateAsync(new ProductoEditViewModel
        {
            Id = id,
            Nombre = entrada.Nombre,
            Categoria = entrada.Categoria,
            PrecioVenta = entrada.PrecioVenta,
            StockMinimo = entrada.StockMinimo!.Value,
            Activo = entrada.Activo!.Value
        }));

    [HttpPost("{id:int:min(1)}/ajustes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Ajustar(int id, AjusteInventarioEntrada entrada) =>
        SinContenidoSiExiste(await productos.AjustarAsync(new ProductoAjusteViewModel
        {
            ProductoId = id,
            Tipo = entrada.Tipo,
            Cantidad = entrada.Cantidad,
            Motivo = entrada.Motivo
        }));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id) =>
        SinContenidoSiExiste(await productos.DesactivarAsync(id));
}
