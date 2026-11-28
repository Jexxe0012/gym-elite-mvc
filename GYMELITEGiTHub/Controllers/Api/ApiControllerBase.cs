using GYMELITEGiTHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Controllers.Api;

[ApiController]
[Authorize(AuthenticationSchemes = ApiConfiguracion.Esquema)]
[TypeFilter(typeof(ApiExceptionFilter))]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult SinContenidoSiExiste(bool existe) => existe ? NoContent() : NotFound();
}

public sealed class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException &&
            context.HttpContext.RequestAborted.IsCancellationRequested)
            return;

        var (status, detail) = context.Exception switch
        {
            ReglaNegocioException { Tipo: TipoRechazo.DatoInvalido } error => (400, error.Message),
            ReglaNegocioException { Tipo: TipoRechazo.ReferenciaInexistente } error => (404, error.Message),
            ReglaNegocioException error => (409, error.Message),
            System.ComponentModel.DataAnnotations.ValidationException error => (400, error.Message),
            DbUpdateConcurrencyException => (409, "El registro cambió durante la operación. Consulte los datos e intente nuevamente."),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } =>
                (409, "Ya existe un registro con los mismos datos únicos."),
            _ => (500, "No se pudo completar la solicitud.")
        };

        if (status == 500)
            logger.LogError(context.Exception, "Error de API en {Accion}. Id de seguimiento: {TraceId}",
                context.ActionDescriptor.DisplayName,
                System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

        var factory = context.HttpContext.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory>();
        context.Result = new ObjectResult(factory.CreateProblemDetails(context.HttpContext, status, detail: detail))
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
        context.ExceptionHandled = true;
    }
}
