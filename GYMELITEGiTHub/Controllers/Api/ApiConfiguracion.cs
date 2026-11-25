using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GYMELITEGiTHub.Controllers.Api;

public static class ApiConfiguracion
{
    public const string Esquema = "Bearer";
    public const string Recepcion = "ApiRecepcion";
    public const string Inventario = "ApiInventario";
    public const string Administracion = "ApiAdministracion";

    public static void AgregarPoliticasApi(this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Recepcion, policy => policy.RequireRole("Administrador", "Recepcion"));
            options.AddPolicy(Inventario, policy => policy.RequireRole("Administrador", "Inventario"));
            options.AddPolicy(Administracion, policy => policy.RequireRole("Administrador"));
        });
        services.AddProblemDetails();
    }

    public static void UsarRespuestasApi(this WebApplication app)
    {
        app.UseWhen(
            context => context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()
                ?.ControllerTypeInfo.IsSubclassOf(typeof(ApiControllerBase)) == true,
            api =>
            {
                api.UseStatusCodePages();
                api.Use(async (context, next) =>
                {
                    // La API queda cerrada hasta que el frente de seguridad registre JWT Bearer.
                    var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                    if (await schemes.GetSchemeAsync(Esquema) is null)
                    {
                        await Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                            detail: "La autenticación de la API aún no está configurada.").ExecuteAsync(context);
                        return;
                    }

                    await next(context);
                });
            });
    }
}
