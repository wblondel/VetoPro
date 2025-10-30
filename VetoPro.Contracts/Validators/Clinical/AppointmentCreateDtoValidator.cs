using FluentValidation;
using VetoPro.Contracts.DTOs.Clinical;

namespace VetoPro.Contracts.Validators.Clinical;

public class AppointmentCreateDtoValidator : AbstractValidator<AppointmentCreateDto>
{
    public AppointmentCreateDtoValidator()
    {
        RuleFor(x => x.StartAt)
            .NotEmpty()
            .WithMessage("L'heure de début est obligatoire.");

        RuleFor(x => x.EndAt)
            .NotEmpty()
            .WithMessage("L'heure de fin est obligatoire.")
            .GreaterThan(x => x.StartAt)
            .WithMessage("L'heure de fin doit être postérieure à l'heure de début."); // <-- Complex rule

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

        // We can add database checks here later if needed (e.g., check if ClientId exists)
        // using dependency injection in the validator constructor.
    }
}