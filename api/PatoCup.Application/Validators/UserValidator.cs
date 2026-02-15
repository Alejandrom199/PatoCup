using FluentValidation;
using PatoCup.Application.DTOs.Security;

namespace PatoCup.Application.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .MaximumLength(50).WithMessage("El usuario no puede tener más de 50 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("Debes ingresar un correo electrónico válido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("Debes asignar un rol válido.");
        }
    }

    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del usuario es inválido.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .MaximumLength(50).WithMessage("El usuario no puede tener más de 50 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("Debes ingresar un correo electrónico válido.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("Debes asignar un rol válido.");

            RuleFor(x => x.StateId)
                .GreaterThan(0).WithMessage("El estado seleccionado no es válido.");

        }
    }
}