using BravoBack.Data;
using BravoBack.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BravoBack.Validators
{
    // Validador para crear un nuevo vehiculo
    public class CreateVehiculoValidator : AbstractValidator<CreateVehiculoDto>
    {
        private readonly AppDbContext _context;

        public CreateVehiculoValidator(AppDbContext context)
        {
            _context = context;

            // Reglas basicas de formato para la placa
            RuleFor(x => x.Placa)
                .NotEmpty().WithMessage("La placa es obligatoria.")
                .Length(6, 10).WithMessage("La placa debe tener entre 6 y 10 caracteres.")
                .Matches(@"^[a-zA-Z0-9-]+$").WithMessage("La placa solo puede contener letras, numeros y guiones.");

            // Marca requerida
            RuleFor(x => x.Marca)
                .NotEmpty().WithMessage("La marca es obligatoria.");

            // Modelo requerido
            RuleFor(x => x.Modelo)
                .NotEmpty().WithMessage("El modelo es obligatorio.");

            // Validacion del rango de anio permitido
            RuleFor(x => x.Anio)
                .InclusiveBetween(1990, DateTime.Now.Year + 1)
                .WithMessage($"El anio debe estar entre 1990 y {DateTime.Now.Year + 1}.");

            // El kilometraje no puede ser negativo
            RuleFor(x => x.KilometrajeActual)
                .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");

            // Intervalo minimo para el servicio
            RuleFor(x => x.IntervaloServicioKm)
                .GreaterThanOrEqualTo(1000).WithMessage("El intervalo de servicio debe ser al menos de 1000 km.");

            // Validacion contra la base de datos para evitar placas duplicadas
            RuleFor(x => x.Placa)
                .MustAsync(PlacaUnica).WithMessage("Ya existe un vehiculo registrado con esta placa.");
        }

        // Checa en la BD si la placa ya fue usada
        private async Task<bool> PlacaUnica(string placa, CancellationToken token)
        {
            // Retorna true si no encuentra ningun vehiculo con esa placa
            return !await _context.Vehiculos.AnyAsync(v => v.Placa == placa, token);
        }
    }

    // Validador para actualizar un vehiculo existente
    public class UpdateVehiculoValidator : AbstractValidator<UpdateVehiculoDto>
    {
        private readonly AppDbContext _context;

        public UpdateVehiculoValidator(AppDbContext context)
        {
            _context = context;

            // La placa debe tener un formato valido
            RuleFor(x => x.Placa)
                .NotEmpty().Length(6, 10);

            // Validacion para evitar usar una placa que ya pertenece a otro vehiculo
            RuleFor(x => x)
                .MustAsync(PlacaUnicaAlEditar).WithMessage("La placa ya pertenece a otro vehiculo.")
                .WithName("Placa"); // Se asocia el error al campo Placa
        }

        // Valida placas duplicadas al editar (permite la misma si pertenece al mismo vehiculo)
        private async Task<bool> PlacaUnicaAlEditar(UpdateVehiculoDto dto, CancellationToken token)
        {
            var existeOtro = await _context.Vehiculos
                .AnyAsync(v => v.Placa == dto.Placa && v.Id != dto.Id, token);

            return !existeOtro;
        }
    }
}
