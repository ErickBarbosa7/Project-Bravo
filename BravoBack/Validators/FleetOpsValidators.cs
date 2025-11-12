using BravoBack.Data;
using BravoBack.DTOs;
using BravoBack.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BravoBack.Validators;

// --- VALIDADOR PARA CHECK-OUT (SALIDA) ---
public class CheckOutDtoValidator : AbstractValidator<CheckOutDto>
{
    private readonly AppDbContext _context;

    public CheckOutDtoValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.VehiculoId)
            .NotEmpty().WithMessage("El ID del vehículo es obligatorio.")
            .MustAsync(VehiculoEsteDisponible)
            .WithMessage("El vehículo no está disponible. Ya está en ruta o en el taller.");
    }

    private async Task<bool> VehiculoEsteDisponible(int vehiculoId, CancellationToken token)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);
        if (vehiculo == null) return false; 
        
        return vehiculo.Estado == EstadoVehiculo.Disponible;
    }
}


// --- VALIDADOR PARA CHECK-IN (ENTRADA) ---
public class CheckInDtoValidator : AbstractValidator<CheckInDto>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CheckInDtoValidator(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.KilometrajeFinal)
            .GreaterThan(0).WithMessage("El kilometraje debe ser positivo.");

        RuleFor(x => x.BitacoraViajeId)
            .NotEmpty().WithMessage("El ID del viaje es obligatorio.")
            .MustAsync(BitacoraSeaValida)
            .WithMessage("El registro de viaje no es válido, no te pertenece o ya fue cerrado.");
            
        RuleFor(dto => dto)
            .MustAsync(KilometrajeSeaValido)
            .WithMessage("El kilometraje final no puede ser menor al de salida.")
            .WithName("KilometrajeFinal"); 
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<bool> BitacoraSeaValida(int bitacoraId, CancellationToken token)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return false;

        return await _context.BitacoraViajes.AnyAsync(b => 
            b.Id == bitacoraId && 
            b.ConductorId == currentUserId && 
            b.FechaLlegada == null, token);
    }
    
    private async Task<bool> KilometrajeSeaValido(CheckInDto dto, CancellationToken token)
    {
        var bitacora = await _context.BitacoraViajes.FindAsync(dto.BitacoraViajeId);
        if (bitacora == null) return true; 

        return dto.KilometrajeFinal >= bitacora.KilometrajeSalida;
    }
}


// --- VALIDADOR PARA SERVICIO COMPLETADO ---
public class ServiceCompletedDtoValidator : AbstractValidator<ServiceCompletedDto>
{
    private readonly AppDbContext _context;

    public ServiceCompletedDtoValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.VehiculoId)
            .NotEmpty().WithMessage("El ID del vehículo es obligatorio.")
            .MustAsync(VehiculoExistaYRequieraServicio)
            .WithMessage("El vehículo no existe o no está marcado como 'NecesitaServicio' o 'EnTaller'.");

        RuleFor(x => x.DescripcionServicio)
            .NotEmpty().WithMessage("La descripción del servicio es obligatoria.")
            .MaximumLength(500);
    }

    private async Task<bool> VehiculoExistaYRequieraServicio(int vehiculoId, CancellationToken token)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(vehiculoId);
        if (vehiculo == null) return false;

        return vehiculo.Estado == EstadoVehiculo.NecesitaServicio || 
               vehiculo.Estado == EstadoVehiculo.EnTaller;
    }
}