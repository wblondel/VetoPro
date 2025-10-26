using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un contact existant.
/// Ne gère pas la modification du compte de connexion (email/mot de passe).
/// </summary>
public class ContactUpdateDto
{
    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Le nom de famille est obligatoire.")]
    [MaxLength(100)]
    public string LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; } // Email de contact (profil)

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
    /// Optionnel : Fournir pour créer ou mettre à jour les détails du staff.
    /// Si 'IsStaff' est 'true', ce champ doit être fourni.
    /// Si 'IsStaff' est 'false', tous les détails existants seront supprimés.
    /// </summary>
    public StaffDetailsUpdateDto? StaffDetails { get; set; }
}