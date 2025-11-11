using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using BravoBack.Data; 

namespace BravoBack.Data; // Puedes ponerlo en el mismo namespace

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // 1. Encontrar el appsettings.json
        // Esto sube dos niveles (desde /Data a /BravoBack) y busca el JSON
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json")
            .Build();

        // 2. Construir las Opciones (Options) manualmente
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // 3. Obtener la cadena de conexión del appsettings
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 4. Decirle que use MySQL
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        // 5. Devolver un nuevo AppDbContext
        return new AppDbContext(optionsBuilder.Options);
    }
}