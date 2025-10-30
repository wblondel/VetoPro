using FluentValidation;
using VetoPro.Contracts.DTOs.Catalogs;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Catalogs; // Correct Validator namespace

public class ColorCreateDtoValidator : AbstractValidator<ColorCreateDto>
{
    public ColorCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de la couleur est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.HexValue)
            .MaximumLength(10).WithMessage("La valeur hexadécimale ne peut pas dépasser 10 caractères.")
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .WithMessage("La valeur hexadécimale doit être un code valide (ex: #FF0000 ou #F00).")
            .When(x => !string.IsNullOrEmpty(x.HexValue)); // Only validate format if provided
    }
}