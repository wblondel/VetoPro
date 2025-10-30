using FluentValidation;
using VetoPro.Contracts.DTOs.Catalogs;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Catalogs; // Correct Validator namespace

public class BreedUpdateDtoValidator : AbstractValidator<BreedUpdateDto>
{
    public BreedUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de la race est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.SpeciesId)
            .NotEmpty().WithMessage("L'ID de l'espèce est obligatoire.");
    }
}