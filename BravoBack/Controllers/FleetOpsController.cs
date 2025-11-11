using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto checkOutDto)
    {
        // 1. Encontrar el vehículo
        var vehiculo = await _context.Vehiculos.FindAsync(checkOutDto.VehiculoId);
        if (vehiculo == null)
        {
            return NotFound(new { message = "Vehículo no encontrado." });
        }

        // 2. Lógica de Negocio: Verificar que esté DISPONIBLE
        if (vehiculo.Estado != EstadoVehiculo.Disponible)
        {
            return BadRequest(new { message = $"El vehículo no está disponible. Estado actual: {vehiculo.Estado}" });
        }

        // 3. Obtener el ID del Conductor desde el Token
        var conductorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 4. Iniciar la transacción
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Paso A: Cambiar el estado del vehículo
            vehiculo.Estado = EstadoVehiculo.EnRuta;
            _context.Vehiculos.Update(vehiculo);

            // Paso B: Crear la nueva bitácora de viaje
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

            // Devolvemos el ticket de bitácora
            return Ok(bitacora);
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
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto checkInDto)
    {
        // 1. Obtener el viaje y su vehículo
        var bitacora = await _context.BitacoraViajes
            .Include(b => b.Vehiculo) 
            .FirstOrDefaultAsync(b => b.Id == checkInDto.BitacoraViajeId);

        if (bitacora == null)
        {
            return NotFound(new { message = "Registro de viaje no encontrado." });
        }

        // 2. Lógica de Negocio: Validar kilometraje
        if (checkInDto.KilometrajeFinal < bitacora.KilometrajeSalida)
        {
            return BadRequest(new { message = "Error: El kilometraje final no puede ser menor al de salida." });
        }

        var vehiculo = bitacora.Vehiculo;

        // 3. Iniciar la transacción
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Paso A: Actualizar la bitácora
            bitacora.FechaLlegada = DateTime.UtcNow;
            bitacora.KilometrajeLlegada = checkInDto.KilometrajeFinal;
            _context.BitacoraViajes.Update(bitacora);

            // Paso B: Actualizar el vehículo
            vehiculo.KilometrajeActual = checkInDto.KilometrajeFinal;

            // Paso C: ¡El Trigger! Revisar si necesita servicio
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
    public async Task<IActionResult> ServiceCompleted([FromBody] ServiceCompletedDto serviceDto)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(serviceDto.VehiculoId);
        if (vehiculo == null)
        {
            return NotFound(new { message = "Vehículo no encontrado." });
        }

        // Iniciar transacción
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Paso A: Actualizar el vehículo
            vehiculo.Estado = EstadoVehiculo.Disponible;
            // ¡Recalcular el próximo servicio!
            vehiculo.SiguienteServicioKm = vehiculo.KilometrajeActual + vehiculo.IntervaloServicioKm;
            _context.Vehiculos.Update(vehiculo);

            // Paso B: Registrar el servicio
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