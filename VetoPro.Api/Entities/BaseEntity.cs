using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.Entities;

/// <summary>
/// Classe de base abstraite pour toutes les entités de la base de données.
/// Fournit un Id (UUID), et les timestamps CreatedAt/UpdatedAt.
/// La génération de l'Id (UUIDv7) et la gestion des timestamps
/// sont gérées dans VetoProDbContext.SaveChangesAsync().
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Clé primaire (UUID).
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Date et heure (UTC) de la création de l'enregistrement.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date et heure (UTC) de la dernière modification de l'enregistrement.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}