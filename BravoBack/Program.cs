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
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN DE BASE DE DATOS
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
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
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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

app.UseHttpsRedirection();

// Configuración de Carpeta de Imágenes (/uploads)
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? app.Environment.ContentRootPath, "wwwroot", "uploads");

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
app.UseCors(myAllowSpecificOrigins);

app.UseAuthentication(); // Quien inicio sesion
app.UseAuthorization();  // Que puedes hacer segun el rol

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
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