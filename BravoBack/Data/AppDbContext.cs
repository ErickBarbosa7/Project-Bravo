using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BravoBack.Models;

namespace BravoBack.Data; 

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<BitacoraViaje> BitacoraViajes { get; set; }
    public DbSet<RegistroServicio> RegistroServicios { get; set; }
    public DbSet<IncidenteReporte> IncidenteReportes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Relaciones
        builder.Entity<Vehiculo>()
            .HasMany(v => v.BitacoraViajes)
            .WithOne(b => b.Vehiculo)
            .HasForeignKey(b => b.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Vehiculo>()
            .HasMany(v => v.RegistrosServicio)
            .WithOne(s => s.Vehiculo)
            .HasForeignKey(s => s.VehiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vehiculo>()
            .HasMany(v => v.Incidentes)
            .WithOne(i => i.Vehiculo)
            .HasForeignKey(i => i.VehiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimales
        builder.Entity<RegistroServicio>().Property(s => s.Costo).HasColumnType("decimal(10, 2)");
        builder.Entity<IncidenteReporte>().Property(i => i.CostoReparacion).HasColumnType("decimal(10, 2)");

        // Enums como Strings
        builder.Entity<Vehiculo>().Property(v => v.Estado).HasConversion<string>();
        builder.Entity<IncidenteReporte>().Property(i => i.TipoIncidente).HasConversion<string>();
        builder.Entity<IncidenteReporte>().Property(i => i.EstadoIncidente).HasConversion<string>();
    }
}