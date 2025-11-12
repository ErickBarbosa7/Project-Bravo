using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FluentValidation; 
using BravoBack.Services;

namespace BravoBack.Controllers;

[ApiController]
[Route("api/fleet")]
[Authorize] 
public class FleetOpsController : ControllerBase
{
    private readonly AppDbContext _context;

    public FleetOpsController(AppDbContext context)
    {
        _context = context;
    }

    // --- Endpoint del Conductor: CHECK-OUT ---
    [HttpPost("checkout")]
    [Authorize(Roles = "Conductor")]
    public async Task<IActionResult> CheckOut(
        [FromBody] CheckOutDto checkOutDto,
        [FromServices] IValidator<CheckOutDto> validator) 
    {
        // --- VALIDACIÓN ASÍNCRONA ---
        var validationResult = await validator.ValidateAsync(checkOutDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var vehiculo = await _context.Vehiculos.FindAsync(checkOutDto.VehiculoId);
        var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            vehiculo.Estado = EstadoVehiculo.EnRuta;
            _context.Vehiculos.Update(vehiculo);

            var bitacora = new BitacoraViaje
            {
                VehiculoId = vehiculo.Id,
                ConductorId = conductorId,
                FechaSalida = DateTime.UtcNow,
                KilometrajeSalida = vehiculo.KilometrajeActual,
                Destino = checkOutDto.Destino
            };
            _context.BitacoraViajes.Add(bitacora);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new 
            { 
                bitacora.Id, 
                bitacora.FechaSalida, 
                bitacora.KilometrajeSalida,
                bitacora.Destino 
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error al registrar la salida.", error = ex.Message });
        }
    }

    // --- Endpoint del Conductor: CHECK-IN ---
    [HttpPost("checkin")]
    [Authorize(Roles = "Conductor")]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInDto checkInDto,
        [FromServices] IValidator<CheckInDto> validator) 
    {
        var validationResult = await validator.ValidateAsync(checkInDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }


        var bitacora = await _context.BitacoraViajes
            .Include(b => b.Vehiculo) 
            .FirstAsync(b => b.Id == checkInDto.BitacoraViajeId); 

        var vehiculo = bitacora.Vehiculo;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            bitacora.FechaLlegada = DateTime.UtcNow;
            bitacora.KilometrajeLlegada = checkInDto.KilometrajeFinal;
            _context.BitacoraViajes.Update(bitacora);

            vehiculo.KilometrajeActual = checkInDto.KilometrajeFinal;

            if (vehiculo.KilometrajeActual >= vehiculo.SiguienteServicioKm)
            {
                vehiculo.Estado = EstadoVehiculo.NecesitaServicio;
            }
            else
            {
                vehiculo.Estado = EstadoVehiculo.Disponible;
            }
            _context.Vehiculos.Update(vehiculo);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = $"Vehículo '{vehiculo.Nombre}' recibido. Nuevo estado: {vehiculo.Estado}" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error al registrar la entrada.", error = ex.Message });
        }
    }

    // --- Endpoint del Gerente: SERVICIO COMPLETADO ---
    [HttpPost("service-completed")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> ServiceCompleted(
        [FromBody] ServiceCompletedDto serviceDto,
        [FromServices] IValidator<ServiceCompletedDto> validator) 
    {
        var validationResult = await validator.ValidateAsync(serviceDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }
        var vehiculo = await _context.Vehiculos.FindAsync(serviceDto.VehiculoId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            vehiculo.Estado = EstadoVehiculo.Disponible;
            vehiculo.SiguienteServicioKm = vehiculo.KilometrajeActual + vehiculo.IntervaloServicioKm;
            _context.Vehiculos.Update(vehiculo);

            var registro = new RegistroServicio
            {
                VehiculoId = vehiculo.Id,
                FechaServicio = DateTime.UtcNow,
                Descripcion = serviceDto.DescripcionServicio,
                Costo = serviceDto.Costo,
                KilometrajeEnServicio = vehiculo.KilometrajeActual
            };
            _context.RegistroServicios.Add(registro);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = $"Servicio completado. Próximo servicio en: {vehiculo.SiguienteServicioKm} km." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error al guardar el servicio.", error = ex.Message });
        }
    }
}