using FluentValidation;
using VetoPro.Contracts.DTOs.Catalogs;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Catalogs; // Correct Validator namespace

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du produit est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Le stock ne peut pas être négatif.");

        RuleFor(x => x.UnitPrice)
            .NotEmpty().WithMessage("Le prix unitaire est obligatoire.")
            .GreaterThanOrEqualTo(0).WithMessage("Le prix ne peut pas être négatif.");

        // Description is optional
    }
}