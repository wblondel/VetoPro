namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un rendez-vous existant.
/// </summary>
public class AppointmentUpdateDto
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public Guid ClientId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? DoctorContactId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; }
}