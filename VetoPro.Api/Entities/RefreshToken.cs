using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VetoPro.Api.Entities;

/// <summary>
/// Stocke un Refresh Token (jeton d'actualisation) pour un utilisateur.
/// Permet une authentification "Rester connecté" sécurisée sur plusieurs appareils.
/// </summary>
public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Le token (une longue chaîne aléatoire).
    /// </summary>
    [Required]
    public string Token { get; set; }

    /// <summary>
    /// L'utilisateur à qui ce token appartient.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    /// <summary>
    /// La date d'expiration du token (longue durée, ex: 30 jours).
    /// </summary>
    [Required]
    public DateTime ExpiresUtc { get; set; }

    /// <summary>
    /// Indique si ce token a été révoqué (ex: déconnexion).
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Date de création (pas besoin de BaseEntity ici,
    /// car la gestion est très spécifique).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}