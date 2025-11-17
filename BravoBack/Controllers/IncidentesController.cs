using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using BravoBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using FluentValidation; 

namespace BravoBack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class IncidentesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IncidentesController(AppDbContext context, IFileService fileService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _fileService = fileService;
        _userManager = userManager;
    }

    // --- Endpoint del Conductor: REPORTAR INCIDENTE ---
    [HttpPost]
    [Authorize(Roles = "Conductor")]
    // Inyectar el validador [FromServices]
    public async Task<IActionResult> ReportarIncidente(
        [FromForm] CreateIncidenteDto incidenteDto,
        [FromServices] IValidator<CreateIncidenteDto> validator)
    {
        // Añadir el bloque de validación
        var validationResult = await validator.ValidateAsync(incidenteDto);
        if (!validationResult.IsValid)
        {
            
            return BadRequest(validationResult.ToDictionary());
        }
        // 1. Obtener el ConductorId desde el Token
        var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (conductorId == null)
        {
            return Unauthorized();
        }

        // 2. Procesar la imagen (si existe)
        string? fotoUrl = null;
        if (incidenteDto.Foto != null)
        {
            fotoUrl = await _fileService.GuardarImagenAsync(incidenteDto.Foto, "incidentes");
        }

        // 3. Iniciar Transacción
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Paso A: Crear el reporte de incidente
            var incidente = new IncidenteReporte
            {
                VehiculoId = incidenteDto.VehiculoId,
                ConductorId = conductorId,
                Fecha = DateTime.UtcNow,
                Descripcion = incidenteDto.Descripcion,
                TipoIncidente = incidenteDto.TipoIncidente,
                FotoUrl = fotoUrl,
                EstadoIncidente = EstadoIncidente.Reportado // Estado inicial
            };
            _context.IncidenteReportes.Add(incidente);

            // Paso B: Actualizar el estado del vehículo
            if (incidente.TipoIncidente == TipoIncidente.Moderado || incidente.TipoIncidente == TipoIncidente.Grave)
            {
               
                var vehiculo = await _context.Vehiculos.FindAsync(incidenteDto.VehiculoId);
                if (vehiculo != null)
                {
                    vehiculo.Estado = EstadoVehiculo.EnTaller;
                    _context.Vehiculos.Update(vehiculo);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Incidente reportado exitosamente.", incidenteId = incidente.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error al reportar el incidente.", error = ex.Message });
        }
    }

    // --- Endpoint del Gerente: VER INCIDENTES DE UN VEHÍCULO ---
    [HttpGet("vehiculo/{vehiculoId}")]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<IEnumerable<IncidenteDto>>> GetIncidentesPorVehiculo(int vehiculoId)
    {
        var incidentes = await _context.IncidenteReportes
            .Where(i => i.VehiculoId == vehiculoId)
            .Include(i => i.Conductor) 
            .Select(i => new IncidenteDto
            {
                Id = i.Id,
                Fecha = i.Fecha,
                Descripcion = i.Descripcion,
                FotoUrl = i.FotoUrl,
                TipoIncidente = i.TipoIncidente.ToString(),
                EstadoIncidente = i.EstadoIncidente.ToString(),
                CostoReparacion = i.CostoReparacion,
                VehiculoId = i.VehiculoId,
                ConductorNombre = $"{i.Conductor.FirstName} {i.Conductor.PaternalLastName} {i.Conductor.MaternalLastName}"

            })
            .OrderByDescending(i => i.Fecha)
            .ToListAsync();

        return Ok(incidentes);
    }
}