using FluentValidation;
using VetoPro.Contracts.DTOs.Auth; // Ou VetoPro.Contracts.DTOs.Auth

namespace VetoPro.Contracts.Validators.Auth; // Ou VetoPro.Contracts.Validators

public class AccountCreateDtoValidator : AbstractValidator<AccountCreateDto>
{
    public AccountCreateDtoValidator()
    {
        RuleFor(x => x.LoginEmail)
            .NotEmpty().WithMessage("L'e-mail de connexion est obligatoire.")
            .EmailAddress().WithMessage("Le format de l'e-mail n'est pas valide.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(6).WithMessage("Le mot de passe doit faire au moins 6 caractères.");
    }
}