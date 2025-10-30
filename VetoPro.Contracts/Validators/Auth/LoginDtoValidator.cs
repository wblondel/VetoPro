using FluentValidation;
using VetoPro.Contracts.DTOs.Auth; // Correct DTO namespace

namespace VetoPro.Contracts.Validators.Auth; // Correct Validator namespace

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'e-mail est obligatoire.")
            .EmailAddress().WithMessage("Le format de l'e-mail n'est pas valide.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.");
    }
}