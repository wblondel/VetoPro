using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

public class StaffDetails : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) liant ces détails au contact principal.
    /// C'est aussi une clé unique pour forcer la relation 1-à-1.
    /// </summary>
    [Required]
    [ForeignKey("Contact")]
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } // Propriété de navigation 1-à-1
    
    /// <summary>
    /// Rôle du membre du personnel (ex: "Veterinarian", "Nurse", "Admin").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Role { get; set; }
    
    /// <summary>
    /// Numéro de licence ou d'ordre (pour les vétérinaires).
    /// </summary>
    [MaxLength(100)]
    public string? LicenseNumber { get; set; }
    
    /// <summary>
    /// Spécialité éventuelle (ex: "Chirurgie", "Dermatologie").
    /// </summary>
    [MaxLength(100)]
    public string? Specialty { get; set; }
    
    /// <summary>
    /// Indique si ce membre du personnel est actuellement actif.
    /// </summary>
    public bool IsActive { get; set; } = true;
}