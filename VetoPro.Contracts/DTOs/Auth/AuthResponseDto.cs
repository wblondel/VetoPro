using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO de réponse pour une authentification (connexion ou inscription) réussie.
/// Contient les tokens JWT et les informations de base de l'utilisateur.
/// </summary>
public class AuthResponseDto
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public IList<string> Roles { get; set; } // Liste des rôles (ex: "Client", "Admin")

    /// <summary>
    /// Le token d'accès JWT (durée de vie courte, ex: 15 min).
    /// </summary>
    [Required]
    public string AccessToken { get; set; }

    /// <summary>
    /// Le token d'actualisation (durée de vie longue, ex: 30 jours).
    /// </summary>
    [Required]
    public string RefreshToken { get; set; }
}