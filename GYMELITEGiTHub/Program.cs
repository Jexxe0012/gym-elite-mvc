
using GYMELITEGiTHub.Controllers.Api;
using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Infraestructura;
using GYMELITEGiTHub.Repositories.Implementations;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Implementations;
using GYMELITEGiTHub.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;



// Registro temporal para errores de arranque (configuración faltante, base inaccesible, etc.).
// Se reemplaza por la configuración completa de RegistroEventos al construir la aplicación.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
var builder = WebApplication.CreateBuilder(args);

// Logging, manejo de excepciones y monitoreo (Etapa 4, apartados 6 y 7)
builder.AgregarRegistroEventos();
builder.Services.AgregarManejoExcepciones();
builder.Services.AgregarMonitoreo();

// Claves de Data Protection persistentes (cookies de sesión y antiforgery válidas tras reiniciar en IIS).
// En Azure App Service no hace falta: la plataforma ya conserva las claves.
var rutaClaves = builder.Configuration["DataProtection:RutaClaves"];
if (!string.IsNullOrWhiteSpace(rutaClaves))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(rutaClaves))
        .SetApplicationName("GYMELITE");
}

builder.Services.AddControllersWithViews();
builder.Services.AgregarDocumentacionApi();

// Base de datos
builder.Services.AddDbContext<GymEliteDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("GymEliteDb")));

// ASP.NET Core Identity
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<GymEliteDbContext>()
    .AddDefaultTokenProviders();

// Configuración de cookies para MVC
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Configuración JWT
var jwtSecret = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("No se configuró Jwt:SecretKey.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("No se configuró Jwt:Issuer.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("No se configuró Jwt:Audience.");

var jwtKey = Convert.FromBase64String(jwtSecret);

if (jwtKey.Length < 32)
    throw new InvalidOperationException("La clave JWT debe contener al menos 32 bytes.");

builder.Services
    .AddAuthentication()
    .AddJwtBearer(ApiConfiguracion.Esquema, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Políticas de autorización existentes de Josué
builder.Services.AgregarPoliticasApi();

// Repositorio genérico y unidad de trabajo
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositorios
builder.Services.AddScoped<ISocioRepository, SocioRepository>();
builder.Services.AddScoped<IMembresiaRepository, MembresiaRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IAccesoRepository, AccesoRepository>();

// Servicios
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMembresiaService, MembresiaService>();
builder.Services.AddScoped<ITipoMembresiaService, TipoMembresiaService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IAccesoService, AccesoService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    // 0. Migraciones al iniciar: solo si el entorno lo habilita (útil en el despliegue).
    if (configuration.GetValue<bool>("BaseDatos:AplicarMigracionesAlIniciar"))
    {
        await services.GetRequiredService<GymEliteDbContext>().Database.MigrateAsync();
        Log.Information("Migraciones de base de datos aplicadas al iniciar.");
    }

    // 1. Crear roles si todavía no existen
    string[] roles =
    {
        "Administrador",
        "Recepcion",
        "Inventario"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var resultado = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!resultado.Succeeded)
            {
                throw new Exception(
                    $"Error creando el rol {role}: " +
                    string.Join(", ", resultado.Errors.Select(e => e.Description)));
            }
        }
    }

    // 2. Obtener credenciales desde User Secrets
    var email = configuration["BootstrapAdmin:Email"];
    var password = configuration["BootstrapAdmin:Password"];

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password))
    {
        throw new InvalidOperationException(
            "Faltan las credenciales BootstrapAdmin en User Secrets.");
    }

    // 3. Buscar al administrador
    var admin = await userManager.FindByEmailAsync(email);

    // 4. Crear el usuario si todavía no existe
    if (admin == null)
    {
        admin = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var resultado = await userManager.CreateAsync(admin, password);

        if (!resultado.Succeeded)
        {
            throw new Exception(
                "Error creando el administrador: " +
                string.Join(", ", resultado.Errors.Select(e => e.Description)));
        }
    }

    // 5. Asignar el rol Administrador si todavía no lo tiene
    if (!await userManager.IsInRoleAsync(admin, "Administrador"))
    {
        var resultado = await userManager.AddToRoleAsync(
            admin, "Administrador");

        if (!resultado.Succeeded)
        {
            throw new Exception(
                "Error asignando el rol Administrador: " +
                string.Join(", ", resultado.Errors.Select(e => e.Description)));
        }
    }
}

// Excepciones no controladas y páginas de estado (todos los entornos)
app.UsarManejoExcepciones();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Una línea de registro por solicitud (no incluye archivos estáticos)
app.UsarRegistroSolicitudes();

app.UseRouting();

app.UsarRespuestasApi();

app.UseAuthentication();
app.UseAuthorization();

// Usuario e id de seguimiento en todos los registros de la solicitud
app.UsarContextoRegistro();

app.MapearMonitoreo();

app.MapControllers();
app.UsarDocumentacionApi();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information("GYM ELITE iniciado en el entorno {Entorno}.", app.Environment.EnvironmentName);

app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException la usan las herramientas de EF (dotnet ef) y no es un error.
    Log.Fatal(ex, "La aplicación no pudo iniciar.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
