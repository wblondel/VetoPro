using FluentValidation;
using VetoPro.Contracts.DTOs.Management;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Management; // Correct Validator namespace

public class StaffDetailsUpdateDtoValidator : AbstractValidator<StaffDetailsUpdateDto>
{
    public StaffDetailsUpdateDtoValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Le rôle est obligatoire.")
            .MaximumLength(100).WithMessage("Le rôle ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.LicenseNumber)
            .MaximumLength(100).WithMessage("Le numéro de licence ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Specialty)
            .MaximumLength(100).WithMessage("La spécialité ne peut pas dépasser 100 caractères.");
    }
}