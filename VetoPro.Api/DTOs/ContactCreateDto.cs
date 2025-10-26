using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la création d'un nouveau contact.
/// Peut inclure optionnellement la création d'un compte de connexion
/// et/ou la création de détails pour le staff.
/// </summary>
public class ContactCreateDto
{
    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Le nom de famille est obligatoire.")]
    [MaxLength(100)]
    public string LastName { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; } // Email de contact (profil)

    [Phone]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? AddressLine1 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [Required]
    public bool IsOwner { get; set; }

    [Required]
    public bool IsClient { get; set; }

    [Required]
    public bool IsStaff { get; set; }

    /// <summary>
    /// Optionnel : Fournir si un compte de connexion doit être créé.
    /// </summary>
    public AccountCreateDto? Account { get; set; }

    /// <summary>
    /// Optionnel : Fournir si 'IsStaff' est 'true'.
    /// </summary>
    public StaffDetailsCreateDto? StaffDetails { get; set; }
}