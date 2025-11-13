using BravoBack.Data;
using BravoBack.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BravoBack.Validators;

public class CreateIncidenteDtoValidator : AbstractValidator<CreateIncidenteDto>
{
    private readonly AppDbContext _context;

    public CreateIncidenteDtoValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres.");

        RuleFor(x => x.VehiculoId)
            .NotEmpty().WithMessage("El ID del vehículo es obligatorio.")
            .MustAsync(VehiculoExista) 
            .WithMessage("El vehículo especificado no existe.");

        RuleFor(x => x.TipoIncidente)
            .IsInEnum().WithMessage("El tipo de incidente no es válido.");
            
        RuleFor(x => x.Foto)
            .Must(ValidarTipoImagen)
            .WithMessage("El archivo no es una imagen válida (solo se permite .jpg, .jpeg, .png).")
            .Must(ValidarTamañoImagen)
            .WithMessage("La imagen es demasiado grande (máximo 5MB).");
    }

    private async Task<bool> VehiculoExista(int vehiculoId, CancellationToken token)
    {
        return await _context.Vehiculos.AnyAsync(v => v.Id == vehiculoId, token);
    }
    
    private bool ValidarTipoImagen(IFormFile? file)
    {
        if (file == null) return true; 

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension == ".jpg" || extension == ".jpeg" || extension == ".png";
    }

    private bool ValidarTamañoImagen(IFormFile? file)
    {
        if (file == null) return true; 
        
        return file.Length <= 5 * 1024 * 1024; // 5MB
    }
}