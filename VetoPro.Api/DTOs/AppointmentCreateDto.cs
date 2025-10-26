using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la création d'un nouveau rendez-vous.
/// </summary>
public class AppointmentCreateDto
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
    /// Statut initial du RDV (ex: "Scheduled").
    /// </summary>
    [Required(ErrorMessage = "Le statut est obligatoire.")]
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Valeur par défaut
}