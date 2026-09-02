using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BravoBack.Models;

namespace BravoBack.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablas principales del sistema
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<CatalogoVehiculo> CatalogoVehiculos { get; set; }
        public DbSet<RegistroServicio> RegistrosServicio { get; set; } 
        public DbSet<BitacoraUso> BitacorasUso { get; set; }
        public DbSet<CategoriaVehiculo> CategoriasVehiculo { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Categories
            builder.Entity<CategoriaVehiculo>().HasData(
                new CategoriaVehiculo { Id = 1, Nombre = "Escolta" },
                new CategoriaVehiculo { Id = 2, Nombre = "Ejecutivo" },
                new CategoriaVehiculo { Id = 3, Nombre = "Carga" },
                new CategoriaVehiculo { Id = 4, Nombre = "Utilitario" }
            );

            // Configuracion de relaciones

            // Relacion entre Vehiculo y registros de servicio
            builder.Entity<Vehiculo>()
                .HasMany(v => v.RegistrosServicio)
                .WithOne(s => s.Vehiculo)
                .HasForeignKey(s => s.VehiculoId)
                .OnDelete(DeleteBehavior.Cascade); // Borra servicios si se elimina el vehiculo

            // Configuracion del tipo decimal en RegistroServicio
            builder.Entity<RegistroServicio>()
                .Property(s => s.MontoPagado)
                .HasColumnType("decimal(10, 2)");

            // Guardar enums como texto en la BD
            builder.Entity<Vehiculo>()
                .Property(v => v.Estado)
                .HasConversion<string>();

            builder.Entity<RegistroServicio>()
                .Property(s => s.Estado);
                //.HasConversion<string>();

            // Seed Data de Catalogo
            builder.Entity<CatalogoVehiculo>().HasData(
                new CatalogoVehiculo { Id = 1, Marca = "Chevrolet", Modelo = "Suburban", Anio = 2024, Categoria = "Escolta", IntervaloServicioKm = 10000, FotoUrl = "assets/vehiculos/vehiculo-suburban.png" },
                new CatalogoVehiculo { Id = 2, Marca = "Chevrolet", Modelo = "Tahoe", Anio = 2023, Categoria = "Escolta", IntervaloServicioKm = 10000, FotoUrl = "assets/vehiculos/vehiculo-placeholder.png" },
                new CatalogoVehiculo { Id = 3, Marca = "Jeep", Modelo = "Grand Cherokee", Anio = 2024, Categoria = "Ejecutivo", IntervaloServicioKm = 12000, FotoUrl = "assets/vehiculos/vehiculo-placeholder.png" },
                new CatalogoVehiculo { Id = 4, Marca = "Toyota", Modelo = "Hilux", Anio = 2022, Categoria = "Carga", IntervaloServicioKm = 15000, FotoUrl = "assets/vehiculos/vehiculo-placeholder.png" },
                new CatalogoVehiculo { Id = 5, Marca = "Nissan", Modelo = "Sentra", Anio = 2024, Categoria = "Utilitario", IntervaloServicioKm = 10000, FotoUrl = "assets/vehiculos/vehiculo-placeholder.png" }
            );
        }
    }
}
