namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour la création d'un nouveau rendez-vous.
/// </summary>
public class AppointmentCreateDto
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public Guid ClientId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? DoctorContactId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Scheduled"; // Valeur par défaut
}