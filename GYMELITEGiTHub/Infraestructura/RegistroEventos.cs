using System.Diagnostics;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace GYMELITEGiTHub.Infraestructura;

// Registro de eventos de la aplicación (Etapa 4, apartado 6).
// Consola para desarrollo y archivo diario para el entorno publicado.
// Nunca se registran contraseñas, tokens, cuerpos de solicitud ni cadenas de conexión.
public static class RegistroEventos
{
    public const string PlantillaArchivo =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{IdSeguimiento}] [{Usuario}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static void AgregarRegistroEventos(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((contexto, servicios, configuracion) =>
        {
            var carpeta = contexto.Configuration["Registro:Carpeta"];
            if (string.IsNullOrWhiteSpace(carpeta))
                carpeta = Path.Combine(contexto.HostingEnvironment.ContentRootPath, "Logs");

            var diasConservados = contexto.Configuration.GetValue("Registro:DiasConservados", 14);

            configuracion
                .ReadFrom.Configuration(contexto.Configuration)
                .ReadFrom.Services(servicios)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Entorno", contexto.HostingEnvironment.EnvironmentName)
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(carpeta, "gymelite-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: diasConservados,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: PlantillaArchivo);
        });
    }

    // Una línea por solicitud: método, ruta (sin query string), estado, duración, usuario e id de seguimiento.
    public static void UsarRegistroSolicitudes(this WebApplication app)
    {
        app.UseSerilogRequestLogging(opciones =>
        {
            opciones.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0} ms";

            opciones.GetLevel = (http, _, excepcion) =>
                excepcion is not null || http.Response.StatusCode >= 500 ? LogEventLevel.Error
                : http.Response.StatusCode >= 400 ? LogEventLevel.Warning
                : LogEventLevel.Information;

            opciones.EnrichDiagnosticContext = (diagnostico, http) =>
            {
                diagnostico.Set("Usuario", NombreUsuario(http));
                diagnostico.Set("IdSeguimiento", IdSeguimiento(http));
            };
        });
    }

    // Agrega usuario e id de seguimiento a todo lo que registren controladores y servicios.
    // Va después de UseAuthorization para que también conozca al usuario del JWT.
    public static void UsarContextoRegistro(this WebApplication app)
    {
        app.Use(async (http, siguiente) =>
        {
            using (LogContext.PushProperty("Usuario", NombreUsuario(http)))
            using (LogContext.PushProperty("IdSeguimiento", IdSeguimiento(http)))
            {
                await siguiente(http);
            }
        });
    }

    // Mismo valor que muestran la página de error y el campo traceId de los errores de la API.
    public static string IdSeguimiento(HttpContext http) =>
        Activity.Current?.Id ?? http.TraceIdentifier;

    private static string NombreUsuario(HttpContext http) =>
        http.User.Identity?.IsAuthenticated == true
            ? http.User.Identity.Name ?? "autenticado"
            : "anonimo";
}
