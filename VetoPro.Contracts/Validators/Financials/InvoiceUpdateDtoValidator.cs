using FluentValidation;
using VetoPro.Contracts.DTOs.Financials;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Financials; // Correct Validator namespace

public class InvoiceUpdateDtoValidator : AbstractValidator<InvoiceUpdateDto>
{
    public InvoiceUpdateDtoValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("L'ID du client (ClientId) est obligatoire.");

        // ConsultationId is optional

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Le numéro de facture (InvoiceNumber) est obligatoire.")
            .MaximumLength(50).WithMessage("Le numéro de facture ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("La date d'émission (IssueDate) est obligatoire.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("La date d'échéance (DueDate) est obligatoire.")
            .GreaterThanOrEqualTo(x => x.IssueDate).WithMessage("La date d'échéance doit être égale ou postérieure à la date d'émission.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Le statut (Status) est obligatoire.")
            .MaximumLength(50).WithMessage("Le statut ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.InvoiceLines)
            .NotEmpty().WithMessage("La facture doit contenir au moins une ligne.")
            .Must(lines => lines != null && lines.Count > 0).WithMessage("La facture doit contenir au moins une ligne."); // Ensure list is not null and has items

        // Validate each item in the InvoiceLines collection using the *update* validator
        RuleForEach(x => x.InvoiceLines)
            .SetValidator(new InvoiceLineUpdateDtoValidator());
    }
}