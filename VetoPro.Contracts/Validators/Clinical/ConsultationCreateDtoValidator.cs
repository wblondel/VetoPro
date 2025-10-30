using FluentValidation;
using VetoPro.Contracts.DTOs.Clinical;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Clinical; // Correct Validator namespace

public class ConsultationCreateDtoValidator : AbstractValidator<ConsultationCreateDto>
{
    public ConsultationCreateDtoValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("L'ID du docteur est obligatoire.");

        RuleFor(x => x.ConsultationDate)
            .NotEmpty().WithMessage("La date de consultation est obligatoire.");
        // Optional: .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La date ne peut pas être dans le futur.");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(0m, 500m).WithMessage("Le poids doit être réaliste (0-500 kg).")
            .When(x => x.WeightKg.HasValue);

        RuleFor(x => x.TemperatureCelsius)
            .InclusiveBetween(30m, 45m).WithMessage("La température doit être réaliste (30-45°C).")
            .When(x => x.TemperatureCelsius.HasValue);

        // ClinicalExam, Diagnosis, Treatment, Prescriptions are optional text fields.
        // Add .MaximumLength() if needed.
    }
}