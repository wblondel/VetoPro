using FluentValidation;
using VetoPro.Contracts.DTOs.Catalogs;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Catalogs; // Correct Validator namespace

public class PriceListCreateDtoValidator : AbstractValidator<PriceListCreateDto>
{
    public PriceListCreateDtoValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("L'ID du service est obligatoire.");

        // SpeciesId is optional, no rule needed unless you want to check if it exists (requires async/DB check)

        RuleFor(x => x.WeightMinKg)
            .GreaterThanOrEqualTo(0).WithMessage("Le poids minimum ne peut pas être négatif.")
            .LessThanOrEqualTo(500).WithMessage("Le poids minimum semble trop élevé.")
            .When(x => x.WeightMinKg.HasValue);

        RuleFor(x => x.WeightMaxKg)
            .GreaterThanOrEqualTo(0).WithMessage("Le poids maximum ne peut pas être négatif.")
            .LessThanOrEqualTo(500).WithMessage("Le poids maximum semble trop élevé.")
            .GreaterThanOrEqualTo(x => x.WeightMinKg) // Max >= Min
            .WithMessage("Le poids maximum doit être supérieur ou égal au poids minimum.")
            .When(x => x.WeightMaxKg.HasValue && x.WeightMinKg.HasValue); // Only when both are provided

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Le montant est obligatoire.")
            .GreaterThanOrEqualTo(0).WithMessage("Le montant ne peut pas être négatif.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La devise est obligatoire.")
            .Length(3).WithMessage("La devise doit être un code ISO à 3 lettres (ex: EUR).");
    }
}