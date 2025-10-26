using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs.Auth;

/// <summary>
/// DTO pour la demande de connexion (login).
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "L'e-mail est obligatoire.")]
    [EmailAddress(ErrorMessage = "Le format de l'e-mail n'est pas valide.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    public string Password { get; set; }
}