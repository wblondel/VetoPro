using FluentValidation;
using VetoPro.Contracts.DTOs.Financials;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Financials; // Correct Validator namespace

public class PaymentCreateDtoValidator : AbstractValidator<PaymentCreateDto>
{
    public PaymentCreateDtoValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("L'ID de la facture (InvoiceId) est obligatoire.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("La date du paiement est obligatoire.");
        // You might add a rule like .LessThanOrEqualTo(DateTime.UtcNow) if payments cannot be in the future.

        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Le montant est obligatoire.")
            .GreaterThan(0).WithMessage("Le montant doit être positif."); // Payments usually must be > 0

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("La méthode de paiement est obligatoire.")
            .MaximumLength(50).WithMessage("La méthode de paiement ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.TransactionId)
            .MaximumLength(255).WithMessage("L'ID de transaction ne peut pas dépasser 255 caractères.");
    }
}