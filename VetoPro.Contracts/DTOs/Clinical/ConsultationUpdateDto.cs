namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une consultation existante.
/// </summary>
public class ConsultationUpdateDto
{
    public Guid DoctorId { get; set; }
    public DateTime ConsultationDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string? ClinicalExam { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescriptions { get; set; }
}