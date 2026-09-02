using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using System.Text;
using System.Text.Json.Serialization; // Agregado para JsonOptions
using BravoBack.Data; 
using BravoBack.Models;
using BravoBack.Services; // Importante para tus servicios
using BravoBack.Middleware;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN DE BASE DE DATOS
// Render inyecta DATABASE_URL automaticamente; tambien soportamos
// ConnectionStrings:DefaultConnection y otras variables comunes.
var connectionString = ConnectionStringResolver.Resolve(builder.Configuration);

if (string.IsNullOrWhiteSpace(connectionString))
{
    // No arrancar con una cadena invalida que "explota" tarde al tocar la BD.
    var logger = LoggerFactory.Create(l => l.AddConsole()).CreateLogger("Startup");
    logger.LogCritical("No se encontro una connection string valida para PostgreSQL. " +
                       "Revisa la variable de entorno DATABASE_URL o ConnectionStrings__DefaultConnection en Render.");
    throw new InvalidOperationException("Falta la connection string de la base de datos.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);


// IDENTITY Y SEGURIDAD (ROLES Y PASSWORD)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


// AUTENTICACION JWT

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});


// INYECCIÓN DE DEPENDENCIAS 
builder.Services.AddScoped<AuthService>();      // auth
builder.Services.AddScoped<VehiculoService>();  // Vehiculos y pagos de servicio
builder.Services.AddScoped<ConductorService>(); 

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssembly(typeof(Program).Assembly);
        config.AutomaticValidationEnabled = false;
    });

var myAllowSpecificOrigins = "_bravoAppPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                    "https://bravofleet.vercel.app",
                    "http://localhost:4200",
                    "http://localhost:5000"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});


// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

// CORS debe ejecutarse antes de redirección HTTPS y antes de los endpoints
app.UseCors(myAllowSpecificOrigins);

app.UseHttpsRedirection();

// Configuración de Carpeta de Imágenes (/uploads)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(); 
// app.UseStaticFiles(new StaticFileOptions
// {
//    FileProvider = new PhysicalFileProvider(uploadsPath),
//    RequestPath = "/uploads"
// });

app.UseRouting();

app.UseAuthentication(); // Quien inicio sesion
app.UseAuthorization();  // Que puedes hacer segun el rol

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Aplicar migraciones pendientes automáticamente
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        // Llamamos al método que acabamos de crear
        await BravoBack.Data.DbSeeder.SeedUsersAndRolesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar datos iniciales.");
    }
}
app.Run();