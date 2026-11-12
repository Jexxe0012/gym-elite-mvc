using GYMELITEGiTHub.Data;
using GYMELITEGiTHub.Repositories.Implementations;
using GYMELITEGiTHub.Repositories.Interfaces;
using GYMELITEGiTHub.Services.Implementations;
using GYMELITEGiTHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Base de datos (Entity Framework Core + SQL Server)
builder.Services.AddDbContext<GymEliteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GymEliteDb")));

// Repositorios y unidad de trabajo
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISocioRepository, SocioRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Servicios de aplicación
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IMembresiaRepository, MembresiaRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IMembresiaService, MembresiaService>();
builder.Services.AddScoped<ITipoMembresiaService, TipoMembresiaService>();
builder.Services.AddScoped<IPagoService, PagoService>();

// Cada módulo nuevo agrega aquí sus propias líneas, por ejemplo:
// builder.Services.AddScoped<IProductoService, ProductoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
