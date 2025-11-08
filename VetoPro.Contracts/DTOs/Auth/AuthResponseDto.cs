namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO de réponse pour une authentification (connexion ou inscription) réussie.
/// Contient les tokens JWT et les informations de base de l'utilisateur.
/// </summary>
public class AuthResponseDto
{
    public string UserId { get; set; }
    
    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public IList<string> Roles { get; set; } // Liste des rôles (ex: "Client", "Admin")

    /// <summary>
    /// Le token d'accès JWT (durée de vie courte, ex: 15 min).
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Le token d'actualisation (durée de vie longue, ex: 30 jours).
    /// </summary>
    public string RefreshToken { get; set; }
}