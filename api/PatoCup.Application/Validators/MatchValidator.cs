using FluentValidation;
using PatoCup.Application.DTOs.Competition;

namespace PatoCup.Application.Validators.Competition
{
    public class CreateMatchValidator : AbstractValidator<CreateMatchDto>
    {
        public CreateMatchValidator()
        {
            RuleFor(x => x.PhaseId).GreaterThan(0).WithMessage("Fase inválida.");
            RuleFor(x => x.Player1Id).GreaterThan(0).WithMessage("Jugador 1 inválido.");
            RuleFor(x => x.Player2Id).GreaterThan(0).WithMessage("Jugador 2 inválido.");

            // No puedes jugar contra ti mismo
            RuleFor(x => x)
                .Must(x => x.Player1Id != x.Player2Id)
                .WithMessage("Los jugadores deben ser diferentes.");
        }
    }

    public class UpdateMatchValidator : AbstractValidator<UpdateMatchDto>
    {
        public UpdateMatchValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id de partida inválido.");
            RuleFor(x => x.Player1Id).GreaterThan(0).WithMessage("Jugador 1 inválido.");
            RuleFor(x => x.Player2Id).GreaterThan(0).WithMessage("Jugador 2 inválido.");
            RuleFor(x => x)
                .Must(x => x.Player1Id != x.Player2Id)
                .WithMessage("Los jugadores deben ser diferentes.");
        }
    }

    public class RegisterMatchResultValidator : AbstractValidator<RegisterMatchResultDto>
    {
        public RegisterMatchResultValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);

            RuleFor(x => x.ScorePlayer1)
                .GreaterThanOrEqualTo(0).WithMessage("El marcador no puede ser negativo.");

            RuleFor(x => x.ScorePlayer2)
                .GreaterThanOrEqualTo(0).WithMessage("El marcador no puede ser negativo.");

        }
    }
}