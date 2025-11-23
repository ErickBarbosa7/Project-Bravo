using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BravoBack.Models;

namespace BravoBack.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablas
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<RegistroServicio> RegistrosServicio { get; set; } 
        public DbSet<BitacoraUso> BitacorasUso { get; set; }
        public DbSet<BitacoraViaje> BitacorasViaje { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // RELACIONES
            
            // Vehiculo <-> BitacoraViaje
            builder.Entity<Vehiculo>()
                .HasMany(v => v.BitacoraViajes)
                .WithOne(b => b.Vehiculo)
                .HasForeignKey(b => b.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehiculo <-> RegistroServicio
            builder.Entity<Vehiculo>()
                .HasMany(v => v.RegistrosServicio)
                .WithOne(s => s.Vehiculo)
                .HasForeignKey(s => s.VehiculoId)
                .OnDelete(DeleteBehavior.Cascade);


            // CONFIGURACIÓN DE DECIMALES 
            
            builder.Entity<RegistroServicio>()
                .Property(s => s.MontoPagado)
                .HasColumnType("decimal(10, 2)");

            // CONFIGURACIÓN DE ENUMS 
            builder.Entity<Vehiculo>()
                .Property(v => v.Estado)
                .HasConversion<string>();

            builder.Entity<RegistroServicio>()
                .Property(s => s.Estado)
                .HasConversion<string>();

        }
    }
}