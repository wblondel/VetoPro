using FluentValidation;
using VetoPro.Contracts.DTOs.Management;
using VetoPro.Contracts.Validators.Auth;

// Correct DTO namespace

namespace VetoPro.Contracts.Validators.Management; // Correct Validator namespace

public class ContactCreateDtoValidator : AbstractValidator<ContactCreateDto>
{
    public ContactCreateDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100).WithMessage("Le prénom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom de famille est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom de famille ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Le format de l'e-mail de contact n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email)); // Only validate format if provided

        RuleFor(x => x.PhoneNumber)
             // Add specific phone number validation if needed (e.g., regex)
            .MaximumLength(30).WithMessage("Le numéro de téléphone est trop long.");

        RuleFor(x => x.AddressLine1)
            .MaximumLength(255).WithMessage("L'adresse ne peut pas dépasser 255 caractères.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("La ville ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Le code postal ne peut pas dépasser 20 caractères.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Le pays ne peut pas dépasser 100 caractères.");

        // Conditional Validation for Account
        RuleFor(x => x.Account)
            .SetValidator(new AccountCreateDtoValidator()!) // Use the specific validator
            .When(x => x.Account != null); // Only run if Account is provided

        // Conditional Validation for Staff Details
        RuleFor(x => x.StaffDetails)
            .NotNull().WithMessage("Les détails du staff (StaffDetails) sont obligatoires si 'IsStaff' est vrai.")
            .SetValidator(new StaffDetailsCreateDtoValidator()!) // Use the specific validator
            .When(x => x.IsStaff); // Only run if IsStaff is true

         RuleFor(x => x.StaffDetails)
             .Null().WithMessage("Les détails du staff (StaffDetails) doivent être nuls si 'IsStaff' est faux.")
             .When(x => !x.IsStaff); // Ensure StaffDetails is null if IsStaff is false
    }
}