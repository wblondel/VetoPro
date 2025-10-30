using FluentValidation;
using VetoPro.Contracts.DTOs.Catalogs;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Catalogs; // Correct Validator namespace

public class ServiceCreateDtoValidator : AbstractValidator<ServiceCreateDto>
{
    public ServiceCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du service est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        // Description is optional, no rule needed unless you add a max length
    }
}