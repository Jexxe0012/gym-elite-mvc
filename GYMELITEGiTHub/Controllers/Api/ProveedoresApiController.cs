using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/proveedores")]
[Authorize(Policy = ApiConfiguracion.Inventario)]
public sealed class ProveedoresApiController(IProveedorService proveedores) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProveedorDto>>> Listar([StringLength(120)] string? busqueda) =>
        await proveedores.GetAllAsync(busqueda);

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<ProveedorDto>> Obtener(int id) =>
        await proveedores.GetByIdAsync(id) is { } proveedor ? Ok(proveedor) : NotFound();

    [HttpPost]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProveedorDto>> Crear(ProveedorCreateViewModel entrada)
    {
        var proveedor = await proveedores.CreateAsync(entrada);
        return CreatedAtAction(nameof(Obtener), new { id = proveedor.Id }, proveedor);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Actualizar(int id, ProveedorCreateViewModel entrada) =>
        SinContenidoSiExiste(await proveedores.UpdateAsync(new ProveedorEditViewModel
        {
            Id = id,
            Nombre = entrada.Nombre,
            Telefono = entrada.Telefono,
            Correo = entrada.Correo
        }));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(int id) =>
        SinContenidoSiExiste(await proveedores.EliminarAsync(id));
}
