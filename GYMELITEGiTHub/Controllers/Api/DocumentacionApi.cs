using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GYMELITEGiTHub.Controllers.Api;

public static class DocumentacionApi
{
    public static void AgregarDocumentacionApi(this IServiceCollection servicios)
    {
        servicios.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GYM ELITE API",
                Version = "v1",
                Description = "Servicios REST para la gestión del gimnasio. Usa Autenticación para obtener un JWT."
            });
            opciones.DocInclusionPredicate((_, descripcion) =>
                descripcion.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true);
            opciones.TagActionsBy(descripcion =>
                [descripcion.ActionDescriptor is ControllerActionDescriptor accion
                    ? NombreModulo(accion.ControllerName)
                    : "API"]);
            opciones.AddSecurityDefinition(ApiConfiguracion.Esquema, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Pega el accessToken obtenido con POST /api/auth/login, sin escribir el prefijo Bearer."
            });
            opciones.OperationFilter<OperacionApiDocumentada>();
        });
    }

    public static void UsarDocumentacionApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        app.UseSwagger();
        app.UseSwaggerUI(opciones =>
        {
            opciones.SwaggerEndpoint("./v1/swagger.json", "GYM ELITE API v1");
            opciones.DocumentTitle = "GYM ELITE API";
        });
    }

    internal static string NombreModulo(string controlador) => controlador switch
    {
        "Auth" => "Autenticación",
        "MembresiasApi" => "Membresías",
        "DiagnosticoApi" => "Diagnóstico",
        _ => controlador.EndsWith("Api", StringComparison.Ordinal) ? controlador[..^3] : controlador
    };
}

public sealed class OperacionApiDocumentada : IOperationFilter
{
    public void Apply(OpenApiOperation operacion, OperationFilterContext contexto)
    {
        if (contexto.ApiDescription.ActionDescriptor is not ControllerActionDescriptor accion) return;

        var modulo = DocumentacionApi.NombreModulo(accion.ControllerName);
        operacion.Summary ??= accion.ActionName switch
        {
            "Login" => "Iniciar sesión y obtener un JWT",
            "Listar" => $"Consultar el listado de {modulo.ToLowerInvariant()}",
            "Obtener" when accion.ControllerName == "ReportesApi" => "Generar el reporte del período",
            "Obtener" => "Consultar el detalle por identificador",
            "ObtenerMembresias" => "Consultar el historial de membresías del socio",
            "Crear" or "Registrar" => "Registrar una operación o un dato nuevo",
            "Actualizar" => "Actualizar un registro existente",
            "Desactivar" => "Desactivar un registro y conservar su historial",
            "Eliminar" => "Eliminar un proveedor sin compras asociadas",
            "Ajustar" => "Registrar un ajuste de existencias",
            "Cancelar" => "Cancelar una membresía",
            "Anular" => "Anular la operación registrada",
            "Historial" => "Consultar el historial de ingresos",
            "Resumen" => "Consultar el resumen de ingresos del día",
            "Verificar" => "Verificar si un socio tiene acceso vigente",
            _ => accion.ActionName
        };

        var metadatos = accion.EndpointMetadata;
        if (metadatos.OfType<IAllowAnonymous>().Any() || !metadatos.OfType<IAuthorizeData>().Any()) return;

        operacion.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(ApiConfiguracion.Esquema, contexto.Document)] = []
            }
        ];

        operacion.Responses ??= new OpenApiResponses();
        operacion.Responses.TryAdd("401", new OpenApiResponse { Description = "JWT ausente, vencido o inválido." });
        operacion.Responses.TryAdd("403", new OpenApiResponse { Description = "El rol del usuario no permite esta operación." });
    }
}
