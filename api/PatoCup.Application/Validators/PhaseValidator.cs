using FluentValidation;
using PatoCup.Application.DTOs.Competition;

namespace PatoCup.Application.Validators.Competition
{
    public class CreatePhaseValidator : AbstractValidator<CreatePhaseDto>
    {
        public CreatePhaseValidator()
        {
            RuleFor(x => x.TournamentId)
                .GreaterThan(0).WithMessage("El ID del torneo es obligatorio y debe ser válido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la fase es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.")
                .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.");
        }
    }

    public class UpdatePhaseValidator : AbstractValidator<UpdatePhaseDto>
    {
        public UpdatePhaseValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID de la fase no es válido.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la fase es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.PhaseStateId)
                .GreaterThan(0).WithMessage("Debes asignar un estado de fase válido (ej: Pendiente, En Progreso).");

            RuleFor(x => x.StateId)
                .GreaterThan(0).WithMessage("Debes asignar un estado general válido (ej: Activo/Inactivo).");
            
            RuleFor(x => x.Sequence)
            .Must(s => s == -1 || s > 0)
            .WithMessage("El orden debe ser -1 (para ocultar de la PatoCup) o un número positivo (1, 2, 3...).");
        }
    }
}