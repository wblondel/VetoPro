using FluentValidation;
using VetoPro.Contracts.DTOs.Auth; // Correct DTO namespace

namespace VetoPro.Contracts.Validators.Auth; // Correct Validator namespace

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'e-mail est obligatoire.")
            .EmailAddress().WithMessage("Le format de l'e-mail n'est pas valide.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(6).WithMessage("Le mot de passe doit faire au moins 6 caractères.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmation du mot de passe est obligatoire.")
            .Equal(x => x.Password).WithMessage("Les mots de passe ne correspondent pas.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100).WithMessage("Le prénom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom de famille est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom de famille ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Le numéro de téléphone est trop long.");
        // Add specific phone number validation if needed (e.g., regex)
    }
}