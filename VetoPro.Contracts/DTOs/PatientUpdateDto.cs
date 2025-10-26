using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un patient existant.
/// </summary>
public class PatientUpdateDto
{
    [Required(ErrorMessage = "Le nom du patient est obligatoire.")]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required(ErrorMessage = "L'ID du propriétaire (OwnerId) est obligatoire.")]
    public Guid OwnerId { get; set; }

    [Required(ErrorMessage = "L'ID de la race (BreedId) est obligatoire.")]
    public Guid BreedId { get; set; }

    [MaxLength(50)]
    public string? ChipNumber { get; set; }

    [Required(ErrorMessage = "La date de début d'estimation de naissance est obligatoire.")]
    public DateOnly DobEstimateStart { get; set; }

    [Required(ErrorMessage = "La date de fin d'estimation de naissance est obligatoire.")]
    public DateOnly DobEstimateEnd { get; set; }

    [Required(ErrorMessage = "Le genre (Gender) est obligatoire.")]
    [MaxLength(20)]
    public string Gender { get; set; }

    [Required(ErrorMessage = "Le statut reproductif (ReproductiveStatus) est obligatoire.")]
    [MaxLength(20)]
    public string ReproductiveStatus { get; set; }

    /// <summary>
    /// Date et heure du décès (si applicable).
    /// Si null, l'animal est considéré comme vivant.
    /// </summary>
    public DateTime? DeceasedAt { get; set; }

    /// <summary>
    /// Liste des IDs des couleurs à associer au patient.
    /// Ceci remplacera la liste existante.
    /// </summary>
    public ICollection<Guid> ColorIds { get; set; } = new List<Guid>();
}