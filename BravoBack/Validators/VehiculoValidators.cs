using BravoBack.Data;
using BravoBack.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BravoBack.Validators;

// --- VALIDADOR PARA CREAR (POST) ---
public class CreateVehiculoDtoValidator : AbstractValidator<CreateVehiculoDto>
{
    private readonly AppDbContext _context;

    // 'forUpdate' nos permite REUTILIZAR estas reglas en el validador de Update
    public CreateVehiculoDtoValidator(AppDbContext context, bool forUpdate = false)
    {
        _context = context; 

        // Regla que reemplaza a [Required] y [StringLength]
        RuleFor(v => v.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

        // Regla que reemplaza a [Required] y [StringLength]
        RuleFor(v => v.Placa)
            .NotEmpty().WithMessage("La placa es obligatoria.")
            .Length(6, 10).WithMessage("La placa debe tener entre 6 y 10 caracteres.");
            
        // Regla que reemplaza tu 'if (AnyAsync...)' manual
        if (!forUpdate)
        {
            RuleFor(v => v.Placa)
                .MustAsync(PlacaNoExista)
                .WithMessage("La placa ya está registrada en el sistema.");
        }
            
        // Reglas de negocio
        RuleFor(v => v.KilometrajeActual)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");
            
        RuleFor(v => v.IntervaloServicioKm)
            .GreaterThan(100).WithMessage("El intervalo de servicio debe ser al menos 100 km.");
    }

    // Método de ayuda para la regla asíncrona (POST)
    private async Task<bool> PlacaNoExista(string placa, CancellationToken token)
    {
        // Devuelve 'true' (es válido) si la placa NO existe
        return !await _context.Vehiculos.AnyAsync(v => v.Placa == placa, token);
    }
}


// --- VALIDADOR PARA ACTUALIZAR (PUT) ---
public class UpdateVehiculoDtoValidator : AbstractValidator<UpdateVehiculoDto>
{
    private readonly AppDbContext _context;

    public UpdateVehiculoDtoValidator(AppDbContext context)
    {
        _context = context;

        // 1. Incluimos todas las reglas simples del validador de "Create"
        // (Nombre, Kilometraje, Intervalo, etc.)
        Include(new CreateVehiculoDtoValidator(_context, forUpdate: true));

        // 2. Añadimos la regla de "Update" para la placa,
        // que es más compleja porque debe ignorar el ID actual.
        RuleFor(dto => dto) 
            .MustAsync(PlacaNoExistaEnOtroVehiculo)
            .WithMessage("La placa ya está registrada en otro vehículo.")
            .WithName("Placa"); // Asigna el error a la propiedad "Placa"
    }

    // Método de ayuda para la regla asíncrona (PUT)
    private async Task<bool> PlacaNoExistaEnOtroVehiculo(UpdateVehiculoDto dto, CancellationToken token)
    {
        // Devuelve 'true' (es válido) si no existe NINGÚN OTRO vehículo
        // (v.Id != dto.Id) con esta misma placa.
        return !await _context.Vehiculos
            .AnyAsync(v => v.Placa == dto.Placa && v.Id != dto.Id, token);
    }
}