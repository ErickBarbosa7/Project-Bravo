using BravoBack.DTOs;
using FluentValidation;

namespace BravoBack.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2);
            RuleFor(x => x.PaternalLastName).NotEmpty().MinimumLength(2);
            
            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress().WithMessage("Formato de correo inválido.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.Role)
                .Must(r => r == "Gerente" || r == "Conductor")
                .WithMessage("El rol solo puede ser 'Gerente' o 'Conductor'.");
        }
    }
}