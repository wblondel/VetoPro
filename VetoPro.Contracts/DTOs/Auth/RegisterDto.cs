using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO pour l'inscription (registration) d'un nouvel utilisateur (client/propriétaire).
/// Combine la création du compte de connexion et du profil Contact.
/// </summary>
public class RegisterDto
{
    [Required(ErrorMessage = "L'e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "Le format de l'e-mail n'est pas valide.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [MinLength(6, ErrorMessage = "Le mot de passe doit faire au moins 6 caractères.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Le prénom est obligatoire.")]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Le nom de famille est obligatoire.")]
    [MaxLength(100)]
    public string LastName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}