namespace VetoPro.Contracts.DTOs.Management;

/// <summary>
/// DTO pour l'affichage complet d'un contact.
/// Inclut les informations de base, l'e-mail de connexion et les détails du staff.
/// </summary>
public class ContactDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// L'e-mail de connexion (depuis le compte Identity).
    /// </summary>
    public string? LoginEmail { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    /// <summary>
    /// L'e-mail de contact (depuis le profil).
    /// </summary>
    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public bool IsOwner { get; set; }
    public bool IsClient { get; set; }
    public bool IsStaff { get; set; }

    /// <summary>
    /// Détails spécifiques du staff (nul si IsStaff = false).
    /// </summary>
    public StaffDetailsDto? StaffDetails { get; set; }
}