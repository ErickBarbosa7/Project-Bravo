using BravoBack.DTOs;
using FluentValidation;

namespace BravoBack.Validators
{
    // Validador para el registro de uso de gasolina o kilometraje
    public class RegistrarUsoValidator : AbstractValidator<RegistrarUsoDto>
    {
        public RegistrarUsoValidator()
        {
            // El id del vehiculo debe ser valido
            RuleFor(x => x.VehiculoId)
                .GreaterThan(0).WithMessage("ID de vehiculo invalido.");

            // Kilometros recorridos deben ser razonables
            RuleFor(x => x.KilometrosRecorridos)
                .GreaterThan(0).WithMessage("Debes haber recorrido al menos 1 km.")
                .LessThan(5000).WithMessage("Es improbable recorrer 5,000 km en una sola carga. Verifica el dato.");

            // Validacion basica para litros consumidos
            RuleFor(x => x.LitrosConsumidos)
                .GreaterThan(0).WithMessage("Los litros deben ser mayor a 0.")
                .LessThanOrEqualTo(120).WithMessage("El límite máximo de carga por transacción es de 120 Litros.")
                .LessThan(500).WithMessage("Excede la capacidad maxima probable de un tanque.");
            
            
        }
    }

    // Validador para pagos de servicios o mantenimientos
    public class PagoServicioValidator : AbstractValidator<PagoServicioDTO>
    {
        public PagoServicioValidator()
        {
            // El id del vehiculo debe existir
            RuleFor(x => x.VehiculoId)
                .GreaterThan(0);

            // Monto debe ser positivo y con dos decimales maximo
            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto del pago debe ser mayor a 0.")
                .ScalePrecision(2, 18).WithMessage("El monto no puede tener mas de 2 decimales.");

            // El concepto debe tener sentido
            RuleFor(x => x.Concepto)
                .NotEmpty().WithMessage("Debes especificar el concepto (ej. Cambio de aceite).")
                .MinimumLength(5).WithMessage("El concepto es muy corto.");
        }
    }
}
