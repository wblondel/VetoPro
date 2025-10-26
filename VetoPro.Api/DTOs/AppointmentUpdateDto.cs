using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un rendez-vous existant.
/// </summary>
public class AppointmentUpdateDto
{
    [Required(ErrorMessage = "L'heure de début est obligatoire.")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "L'heure de fin est obligatoire.")]
    public DateTime EndAt { get; set; }

    [Required(ErrorMessage = "L'ID du client est obligatoire.")]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "L'ID du patient est obligatoire.")]
    public Guid PatientId { get; set; }

    /// <summary>
    /// ID optionnel du docteur assigné.
    /// </summary>
    public Guid? DoctorContactId { get; set; }

    [MaxLength(255)]
    public string? Reason { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Statut du RDV (ex: "Confirmed", "Completed", "Cancelled").
    /// </summary>
    [Required(ErrorMessage = "Le statut est obligatoire.")]
    [MaxLength(50)]
    public string Status { get; set; }
}