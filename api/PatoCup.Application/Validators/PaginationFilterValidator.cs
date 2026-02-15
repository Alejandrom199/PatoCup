using FluentValidation;
using PatoCup.Application.DTOs.Common;

namespace PatoCup.Application.Validators
{
    public class PaginationFilterValidator : AbstractValidator<PaginationFilterDto>
    {
        public PaginationFilterValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0.")
                .LessThanOrEqualTo(100).WithMessage("El tamaño máximo de página es 100.");
        }
    }
}
