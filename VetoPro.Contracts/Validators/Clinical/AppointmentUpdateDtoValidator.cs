using FluentValidation;
using VetoPro.Contracts.DTOs.Clinical;

// Ou VetoPro.Contracts.DTOs

namespace VetoPro.Contracts.Validators.Clinical; // Ou VetoPro.Contracts.Validators

public class AppointmentUpdateDtoValidator : AbstractValidator<AppointmentUpdateDto>
{
    public AppointmentUpdateDtoValidator()
    {
        RuleFor(x => x.StartAt)
            .NotEmpty()
            .WithMessage("L'heure de début est obligatoire.");

        RuleFor(x => x.EndAt)
            .NotEmpty()
            .WithMessage("L'heure de fin est obligatoire.")
            .GreaterThan(x => x.StartAt)
            .WithMessage("L'heure de fin doit être postérieure à l'heure de début.");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("L'ID du client est obligatoire.");

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("L'ID du patient est obligatoire.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Le statut est obligatoire.")
            .MaximumLength(50)
            .WithMessage("Le statut ne peut pas dépasser 50 caractères.");
            
        RuleFor(x => x.Reason)
            .MaximumLength(255)
            .WithMessage("Le motif ne peut pas dépasser 255 caractères.");
    }
}