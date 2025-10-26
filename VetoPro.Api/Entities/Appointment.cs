using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente un rendez-vous planifié dans l'agenda.
/// </summary>
public class Appointment : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers le contact client qui a pris le RDV.
    /// </summary>
    [Required]
    //[ForeignKey("Client")]
    public Guid ClientId { get; set; }
    public Contact Client { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (obligatoire) vers le patient concerné par le RDV.
    /// </summary>
    [Required]
    [ForeignKey("Patient")]
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (optionnelle) vers le docteur assigné à ce RDV.
    /// </summary>
    //[ForeignKey("Doctor")]
    public Guid? DoctorContactId { get; set; }
    public Contact? Doctor { get; set; } // Propriété de navigation

    /// <summary>
    /// Date et heure (UTC) de début du rendez-vous.
    /// </summary>
    [Required]
    public DateTime StartAt { get; set; }

    /// <summary>
    /// Date et heure (UTC) de fin du rendez-vous.
    /// </summary>
    [Required]
    public DateTime EndAt { get; set; }

    /// <summary>
    /// Motif du rendez-vous (ex: "Vaccin", "Contrôle").
    /// </summary>
    [MaxLength(255)]
    public string? Reason { get; set; }

    /// <summary>
    /// Notes prises lors de la prise de rendez-vous.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Statut du rendez-vous (ex: "Scheduled", "Confirmed", "Completed", "Cancelled", "NoShow").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; }


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Relation 1-à-1 vers la consultation qui découle de ce rendez-vous.
    /// </summary>
    public Consultation? Consultation { get; set; }
}