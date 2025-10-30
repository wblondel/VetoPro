namespace VetoPro.Contracts.DTOs.Management;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un contact existant.
/// Ne gère pas la modification du compte de connexion (email/mot de passe).
/// </summary>
public class ContactUpdateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; } // Email de contact (profil)
    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsOwner { get; set; }
    public bool IsClient { get; set; }
    public bool IsStaff { get; set; }
    public StaffDetailsUpdateDto? StaffDetails { get; set; }
}