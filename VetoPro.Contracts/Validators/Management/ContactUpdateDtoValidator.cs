using FluentValidation;
using VetoPro.Contracts.DTOs.Management;

// Espace de noms correct pour les DTOs

namespace VetoPro.Contracts.Validators.Management; // Espace de noms correct pour les validateurs

public class ContactUpdateDtoValidator : AbstractValidator<ContactUpdateDto>
{
    public ContactUpdateDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est obligatoire.")
            .MaximumLength(100).WithMessage("Le prénom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom de famille est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom de famille ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Le format de l'e-mail de contact n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.Email)); // Valider seulement si fourni

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Le numéro de téléphone est trop long.");

        RuleFor(x => x.AddressLine1)
            .MaximumLength(255).WithMessage("L'adresse ne peut pas dépasser 255 caractères.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("La ville ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Le code postal ne peut pas dépasser 20 caractères.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Le pays ne peut pas dépasser 100 caractères.");

        // Validation conditionnelle pour StaffDetails (identique à la création)
        RuleFor(x => x.StaffDetails)
            .NotNull().WithMessage("Les détails du staff (StaffDetails) sont obligatoires si 'IsStaff' est vrai.")
            .SetValidator(new StaffDetailsUpdateDtoValidator()!) // Utiliser le validateur de mise à jour
            .When(x => x.IsStaff);

         RuleFor(x => x.StaffDetails)
             .Null().WithMessage("Les détails du staff (StaffDetails) doivent être nuls si 'IsStaff' est faux.")
             .When(x => !x.IsStaff);

        // Pas de validation pour le compte (Account) ici, car la mise à jour
        // de l'e-mail/mot de passe doit passer par un flux d'authentification dédié.
    }
}