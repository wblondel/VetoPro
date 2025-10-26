using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente la consultation médicale (l'examen) qui découle d'un rendez-vous.
/// Contient les observations cliniques, diagnostics et traitements.
/// </summary>
public class Consultation : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers le RDV.
    /// Doit être unique pour forcer la relation 1-à-1.
    /// </summary>
    [Required]
    [ForeignKey("Appointment")]
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } // Propriété de navigation 1-à-1

    /// <summary>
    /// Clé étrangère (obligatoire) vers le patient (redondant avec RDV mais bon pour les requêtes).
    /// </summary>
    [Required]
    [ForeignKey("Patient")]
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (obligatoire) vers le client *présent* à la consultation.
    /// </summary>
    [Required]
    //[ForeignKey("Client")]
    public Guid ClientId { get; set; }
    public Contact Client { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (obligatoire) vers le docteur qui a *effectué* la consultation.
    /// </summary>
    [Required]
    //[ForeignKey("Doctor")]
    public Guid DoctorId { get; set; }
    public Contact Doctor { get; set; } // Propriété de navigation

    /// <summary>
    /// Date et heure (UTC) réelles de la consultation (peut différer du RDV).
    /// </summary>
    [Required]
    public DateTime ConsultationDate { get; set; }

    /// <summary>
    /// Poids de l'animal en kg.
    /// </summary>
    [Column(TypeName = "decimal(6, 2)")] // ex: 9999.99
    public decimal? WeightKg { get; set; }

    /// <summary>
    /// Température de l'animal en degrés Celsius.
    /// </summary>
    [Column(TypeName = "decimal(4, 1)")] // ex: 999.9
    public decimal? TemperatureCelsius { get; set; }

    /// <summary>
    /// Compte-rendu de l'examen clinique.
    /// </summary>
    public string? ClinicalExam { get; set; }

    /// <summary>
    /// Diagnostic(s) établi(s) par le docteur.
    /// </summary>
    public string? Diagnosis { get; set; }

    /// <summary>
    /// Traitement(s) administré(s) ou prescrit(s).
    /// </summary>
    public string? Treatment { get; set; }

    /// <summary>
    /// Prescriptions médicamenteuses.
    /// </summary>
    public string? Prescriptions { get; set; }


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Facture(s) générée(s) suite à cette consultation.
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}