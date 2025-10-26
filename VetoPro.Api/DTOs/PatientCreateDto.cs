using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la création d'un nouveau patient.
/// </summary>
public class PatientCreateDto
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
    /// Liste des IDs des couleurs à associer au patient.
    /// </summary>
    public ICollection<Guid> ColorIds { get; set; } = new List<Guid>();
}