using FluentValidation;
using PatoCup.Application.DTOs.Audit;

namespace PatoCup.Application.Validators.Audit
{
    public class CreateAuditLogValidator : AbstractValidator<CreateAuditLogDto>
    {
        public CreateAuditLogValidator()
        {
            RuleFor(x => x.ActionType)
                .NotEmpty().WithMessage("El tipo de acción es obligatorio.")
                .MaximumLength(50).WithMessage("El tipo de acción no puede exceder los 50 caracteres.");

            RuleFor(x => x.IPAddress)
                .NotEmpty().WithMessage("La dirección IP es obligatoria.")
                .MaximumLength(50).WithMessage("La dirección IP no puede exceder los 50 caracteres.");

            RuleFor(x => x.Message)
                .NotNull().WithMessage("El mensaje no puede ser nulo.");

            RuleFor(x => x.UserId)
                .GreaterThan(0).When(x => x.UserId.HasValue)
                .WithMessage("El ID de usuario debe ser un número positivo.");
        }
    }
}