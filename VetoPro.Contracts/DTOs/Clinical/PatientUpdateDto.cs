using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un patient existant.
/// </summary>
public class PatientUpdateDto
{
    public string Name { get; set; }
    public Guid OwnerId { get; set; }
    public Guid BreedId { get; set; }
    public string? ChipNumber { get; set; }
    public DateOnly DobEstimateStart { get; set; }
    public DateOnly DobEstimateEnd { get; set; }
    public string Gender { get; set; }
    public string ReproductiveStatus { get; set; }
    public DateTime? DeceasedAt { get; set; }
    public ICollection<Guid> ColorIds { get; set; } = new List<Guid>();
}