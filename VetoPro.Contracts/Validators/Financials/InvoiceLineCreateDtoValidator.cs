using FluentValidation;
using VetoPro.Contracts.DTOs.Financials;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Financials; // Correct Validator namespace

public class InvoiceLineCreateDtoValidator : AbstractValidator<InvoiceLineCreateDto>
{
    public InvoiceLineCreateDtoValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("L'ID de l'article (ItemId) est obligatoire.");

        RuleFor(x => x.ItemType)
            .NotEmpty().WithMessage("Le type d'article (ItemType) est obligatoire.")
            .Must(type => type.Equals("Service", StringComparison.OrdinalIgnoreCase) || type.Equals("Product", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Le type d'article doit être 'Service' ou 'Product'.");

        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("La quantité est obligatoire.")
            .GreaterThan(0).WithMessage("La quantité doit être positive.");

        RuleFor(x => x.UnitPrice)
            .NotEmpty().WithMessage("Le prix unitaire est obligatoire.")
            .GreaterThanOrEqualTo(0).WithMessage("Le prix ne peut pas être négatif.");
    }
}