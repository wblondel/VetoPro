using FluentValidation;
using VetoPro.Contracts.DTOs.Clinical;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Clinical; // Correct Validator namespace

public class PatientCreateDtoValidator : AbstractValidator<PatientCreateDto>
{
    public PatientCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du patient est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("L'ID du propriétaire est obligatoire.");

        RuleFor(x => x.BreedId)
            .NotEmpty().WithMessage("L'ID de la race est obligatoire.");

        RuleFor(x => x.ChipNumber)
            .MaximumLength(50).WithMessage("Le numéro de puce ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.DobEstimateStart)
            .NotEmpty().WithMessage("La date de début d'estimation de naissance est obligatoire.");

        RuleFor(x => x.DobEstimateEnd)
            .NotEmpty().WithMessage("La date de fin d'estimation de naissance est obligatoire.")
            .GreaterThanOrEqualTo(x => x.DobEstimateStart)
            .WithMessage("La date de fin doit être égale ou postérieure à la date de début.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Le genre est obligatoire.")
            .MaximumLength(20).WithMessage("Le genre ne peut pas dépasser 20 caractères.")
            .Must(g => g.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                       g.Equals("Female", StringComparison.OrdinalIgnoreCase) ||
                       g.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Le genre doit être 'Male', 'Female', ou 'Unknown'.");

        RuleFor(x => x.ReproductiveStatus)
            .NotEmpty().WithMessage("Le statut reproductif est obligatoire.")
            .MaximumLength(20).WithMessage("Le statut reproductif ne peut pas dépasser 20 caractères.")
            .Must(s => s.Equals("Intact", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("Neutered", StringComparison.OrdinalIgnoreCase) || // Castré
                       s.Equals("Spayed", StringComparison.OrdinalIgnoreCase))    // Stérilisée (femelle)
            .WithMessage("Le statut reproductif doit être 'Intact', 'Neutered', ou 'Spayed'.");

        // ColorIds is a collection, no rules needed here unless you require at least one.
        // The controller validates if the IDs exist in the database.
    }
}