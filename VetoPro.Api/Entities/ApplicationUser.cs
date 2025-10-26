using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace VetoPro.Api.Entities;

/// <summary>
/// Entité représentant un utilisateur pouvant se connecter à l'application.
/// Gérée par ASP.NET Identity (stocke le hash du mot de passe, l'email de connexion, etc.).
/// Utilise un Guid comme clé primaire (au lieu d'un string par défaut).
/// La liaison vers le profil métier (Contact) est gérée par l'entité Contact elle-même.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // On peut ajouter des champs ici si nécessaire,
    // mais le profil est déjà dans la table "Contact".
    
    // La propriété de navigation vers Contact a été supprimée d'ici
    // pour éviter une référence circulaire et simplifier la conception.
    // C'est 'Contact.UserId' qui fait autorité.
}