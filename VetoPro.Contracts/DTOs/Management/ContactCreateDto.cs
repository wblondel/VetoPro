using VetoPro.Contracts.DTOs.Auth;

namespace VetoPro.Contracts.DTOs.Management;

/// <summary>
/// DTO pour la création d'un nouveau contact.
/// Peut inclure optionnellement la création d'un compte de connexion
/// et/ou la création de détails pour le staff.
/// </summary>
public class ContactCreateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsOwner { get; set; }
    public bool IsClient { get; set; }
    public bool IsStaff { get; set; }
    public AccountCreateDto? Account { get; set; }
    public StaffDetailsCreateDto? StaffDetails { get; set; }
}