using GYMELITEGiTHub.Models;
using Microsoft.EntityFrameworkCore;

namespace GYMELITEGiTHub.Data;

public class GymEliteDbContext : DbContext
{
    public GymEliteDbContext(DbContextOptions<GymEliteDbContext> options) : base(options) { }

    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<TipoMembresia> TiposMembresia => Set<TipoMembresia>();
    public DbSet<Membresia> Membresias => Set<Membresia>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Acceso> Accesos => Set<Acceso>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Índices únicos
        b.Entity<Socio>().HasIndex(s => s.Codigo).IsUnique();
        b.Entity<Socio>().HasIndex(s => s.CodigoQr).IsUnique();
        b.Entity<Producto>().HasIndex(p => p.Codigo).IsUnique();

        // Precisión de importes
        b.Entity<TipoMembresia>().Property(t => t.Precio).HasPrecision(10, 2);
        b.Entity<Pago>().Property(p => p.Monto).HasPrecision(10, 2);
        b.Entity<Producto>().Property(p => p.PrecioVenta).HasPrecision(10, 2);
        b.Entity<DetalleCompra>().Property(d => d.CostoUnitario).HasPrecision(10, 2);
        b.Entity<DetalleVenta>().Property(d => d.PrecioAplicado).HasPrecision(10, 2);
        b.Entity<Venta>().Property(v => v.Total).HasPrecision(10, 2);

        // El historial no se borra en cascada: se restringe el borrado físico
        b.Entity<Membresia>().HasOne(m => m.Socio).WithMany(s => s.Membresias)
            .HasForeignKey(m => m.SocioId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Membresia>().HasOne(m => m.TipoMembresia).WithMany()
            .HasForeignKey(m => m.TipoMembresiaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Pago>().HasOne(p => p.Membresia).WithMany(m => m.Pagos)
            .HasForeignKey(p => p.MembresiaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Acceso>().HasOne(a => a.Membresia).WithMany(m => m.Accesos)
            .HasForeignKey(a => a.MembresiaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Compra>().HasOne(c => c.Proveedor).WithMany(p => p.Compras)
            .HasForeignKey(c => c.ProveedorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DetalleCompra>().HasOne(d => d.Producto).WithMany(p => p.DetallesCompra)
            .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DetalleVenta>().HasOne(d => d.Producto).WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<MovimientoInventario>().HasOne(m => m.Producto).WithMany(p => p.Movimientos)
            .HasForeignKey(m => m.ProductoId).OnDelete(DeleteBehavior.Restrict);

        // Los detalles sí desaparecen junto con su compra o venta
        b.Entity<DetalleCompra>().HasOne(d => d.Compra).WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CompraId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<DetalleVenta>().HasOne(d => d.Venta).WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId).OnDelete(DeleteBehavior.Cascade);

        // Datos iniciales (precios de los mockups; ajústenlos a los reales)
        b.Entity<TipoMembresia>().HasData(
            new TipoMembresia { Id = 1, Nombre = "Mensual",     Modalidad = "Mensualidad", DuracionDias = 30,  Precio = 25m },
            new TipoMembresia { Id = 2, Nombre = "Trimestral",  Modalidad = "Mensualidad", DuracionDias = 90,  Precio = 65m },
            new TipoMembresia { Id = 3, Nombre = "Semestral",   Modalidad = "Mensualidad", DuracionDias = 180, Precio = 120m },
            new TipoMembresia { Id = 4, Nombre = "Anual",       Modalidad = "Mensualidad", DuracionDias = 365, Precio = 220m },
            new TipoMembresia { Id = 5, Nombre = "Pase diario", Modalidad = "Pase diario", DuracionDias = 1,   Precio = 5m });
    }
}
