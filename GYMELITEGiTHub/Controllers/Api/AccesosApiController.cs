using System.ComponentModel.DataAnnotations;
using GYMELITEGiTHub.Dtos;
using GYMELITEGiTHub.Services.Interfaces;
using GYMELITEGiTHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GYMELITEGiTHub.Controllers.Api;

[Route("api/accesos")]
[Authorize(Policy = ApiConfiguracion.Recepcion)]
public sealed class AccesosApiController(IAccesoService accesos) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AccesoDto>>> Historial(
        [FromQuery] PeriodoApi periodo, [StringLength(100)] string? busqueda)
    {
        var lista = await accesos.ObtenerHistorialAsync(new AccesoHistorialViewModel
        {
            FechaDesde = periodo.FechaDesde,
            FechaHasta = periodo.FechaHasta,
            Busqueda = busqueda
        });
        return lista.Accesos;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<AccesosResumenRespuesta>> Resumen()
    {
        var modelo = new AccesoVerificarViewModel();
        await accesos.PrepararAsync(modelo);
        return new AccesosResumenRespuesta(modelo.IngresosHoy, modelo.SociosHoy, modelo.Recientes);
    }

    [HttpGet("verificacion")]
    public async Task<ActionResult<VerificacionAccesoDto>> Verificar(
        [Required, StringLength(100)] string? busqueda) => await accesos.VerificarAsync(busqueda);

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<AccesoDto>> Obtener(int id) =>
        await accesos.GetByIdAsync(id) is { } acceso ? Ok(acceso) : NotFound();

    [HttpPost]
    [ProducesResponseType<AccesoDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AccesoDto>> Registrar(IdentificacionSocio entrada)
    {
        var acceso = await accesos.RegistrarIngresoAsync(entrada.SocioId);
        return CreatedAtAction(nameof(Obtener), new { id = acceso.Id }, acceso);
    }
}
