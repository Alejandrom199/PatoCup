using FluentValidation;
using PatoCup.Application.DTOs.Competition;

namespace PatoCup.Application.Validators.Competition
{
    public class PublicSubmitPlayerValidator : AbstractValidator<PublicSubmitPlayerDto>
    {
        public PublicSubmitPlayerValidator()
        {
            RuleFor(x => x.Nickname)
                .NotEmpty().WithMessage("El Nickname es obligatorio.")
                .MaximumLength(50).WithMessage("El Nickname no puede exceder los 50 caracteres.")
                .MinimumLength(3).WithMessage("El Nickname debe tener al menos 3 caracteres."); // Evita nombres como "a" o ".."

            RuleFor(x => x.GameId)
                .NotEmpty().WithMessage("El ID del juego es obligatorio.")
                .MaximumLength(20).WithMessage("El ID del juego no puede exceder los 20 caracteres.");
        }
    }

    public class UpdatePlayerValidator : AbstractValidator<UpdatePlayerDto>
    {
        public UpdatePlayerValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del jugador no es válido.");

            RuleFor(x => x.Nickname)
                .NotEmpty().WithMessage("El Nickname es obligatorio.")
                .MaximumLength(50).WithMessage("El Nickname no puede exceder los 50 caracteres.");

            RuleFor(x => x.GameId)
                .NotEmpty().WithMessage("El ID del juego es obligatorio.")
                .MaximumLength(20).WithMessage("El ID del juego no puede exceder los 20 caracteres.");

            RuleFor(x => x.StateId)
                .GreaterThan(0).WithMessage("Debes seleccionar un estado válido.")
                .LessThan(100).WithMessage("ID de estado fuera de rango."); 
        }
    }
}