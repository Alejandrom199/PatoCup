using FluentValidation;
using PatoCup.Application.DTOs.Competition;

namespace PatoCup.Application.Validators.Competition
{
    public class CreateTournamentValidator : AbstractValidator<CreateTournamentDto>
    {
        public CreateTournamentValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del torneo es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(500).WithMessage("La descripción no puede exceder los 500 caracteres.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("La fecha de inicio es obligatoria.")
                .Must(date => date.Date >= DateTime.Today).WithMessage("La fecha de inicio no puede ser en el pasado.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("La fecha de fin es obligatoria.")
                .GreaterThan(x => x.StartDate).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

        }
    }

    public class UpdateTournamentValidator : AbstractValidator<UpdateTournamentDto>
    {
        public UpdateTournamentValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID es necesario para actualizar.")
                .GreaterThan(0).WithMessage("ID de torneo inválido.");

            RuleFor(x => x.StateId)
                .GreaterThan(0).WithMessage("Debes asignar un estado válido.");
        }
    }
}