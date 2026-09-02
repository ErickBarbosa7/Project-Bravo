using BravoBack.DTOs;
using FluentValidation;

namespace BravoBack.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty().MinimumLength(2);
            RuleFor(x => x.PaternalLastName).NotNull().NotEmpty().MinimumLength(2);

            RuleFor(x => x.Email)
                .NotNull().NotEmpty().EmailAddress().WithMessage("Formato de correo inválido.");

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.Role)
                .NotNull()
                .Must(r => r == "Gerente" || r == "Conductor")
                .WithMessage("El rol solo puede ser 'Gerente' o 'Conductor'.");
        }
    }
}