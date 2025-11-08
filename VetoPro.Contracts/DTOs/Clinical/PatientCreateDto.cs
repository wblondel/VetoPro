namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour la création d'un nouveau patient.
/// </summary>
public class PatientCreateDto
{
    public string Name { get; set; }
    public Guid OwnerId { get; set; }
    public Guid BreedId { get; set; }
    public string? ChipNumber { get; set; }
    public DateOnly DobEstimateStart { get; set; }
    public DateOnly DobEstimateEnd { get; set; }
    public string Gender { get; set; }
    public string ReproductiveStatus { get; set; }
    public ICollection<Guid> ColorIds { get; set; } = new List<Guid>();
}