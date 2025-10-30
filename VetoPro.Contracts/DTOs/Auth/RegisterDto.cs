using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO pour l'inscription (registration) d'un nouvel utilisateur (client/propriétaire).
/// Combine la création du compte de connexion et du profil Contact.
/// </summary>
public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
}