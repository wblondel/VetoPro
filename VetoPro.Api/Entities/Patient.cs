using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Entité représentant l'animal, le patient de la clinique.
/// </summary>
public class Patient : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers le propriétaire de l'animal.
    /// </summary>
    [Required]
    [ForeignKey("Owner")]
    public Guid OwnerId { get; set; }
    public Contact Owner { get; set; } // Propriété de navigation
    
    /// <summary>
    /// Clé étrangère (obligatoire) vers la race de l'animal.
    /// </summary>
    [Required]
    [ForeignKey("Breed")]
    public Guid BreedId { get; set; }
    public Breed Breed { get; set; } // Propriété de navigation
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    /// <summary>
    /// Numéro d'identification de la puce électronique (doit être unique).
    /// </summary>
    [MaxLength(50)]
    public string? ChipNumber { get; set; }
    
    /// <summary>
    /// Début de la plage estimée de la date de naissance.
    /// Si la date est connue, égal à DobEstimateEnd.
    /// </summary>
    [Required]
    public DateOnly DobEstimateStart { get; set; }

    /// <summary>
    /// Fin de la plage estimée de la date de naissance.
    /// Si la date est connue, égal à DobEstimateStart.
    /// </summary>
    [Required]
    public DateOnly DobEstimateEnd { get; set; }
    
    /// <summary>
    /// Sexe de l'animal (ex: "Male", "Female", "Unknown").
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Gender { get; set; }

    /// <summary>
    /// Statut reproductif (ex: "Intact", "Neutered", "Spayed").
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string ReproductiveStatus { get; set; }

    /// <summary>
    /// Date et heure du décès (si applicable).
    /// </summary>
    public DateTime? DeceasedAt { get; set; }
    
    // -- Propriétés de Navigation --
    
    /// <summary>
    /// Liste des couleurs de l'animal (relation plusieurs-à-plusieurs).
    /// </summary>
    public ICollection<Color> Colors { get; set; } = new List<Color>();
    
    /// <summary>
    /// Historique des rendez-vous pour ce patient.
    /// </summary>
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    
    /// <summary>
    /// Historique des consultations pour ce patient.
    /// </summary>
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
}