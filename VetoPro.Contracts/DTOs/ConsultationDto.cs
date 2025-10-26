using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour l'affichage complet d'une consultation (compte-rendu médical).
/// </summary>
public class ConsultationDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID du rendez-vous parent.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Date et heure réelles de la consultation.
    /// </summary>
    [Required]
    public DateTime ConsultationDate { get; set; }

    // --- Personnes Impliquées ---
    public Guid ClientId { get; set; }
    [Required]
    public string ClientName { get; set; } // Nom du client présent

    public Guid PatientId { get; set; }
    [Required]
    public string PatientName { get; set; } // Nom du patient

    public Guid DoctorId { get; set; }
    [Required]
    public string DoctorName { get; set; } // Nom du docteur

    // --- Constantes (Vitals) ---
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureCelsius { get; set; }

    // --- Compte-rendu Clinique ---
    public string? ClinicalExam { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescriptions { get; set; }
}