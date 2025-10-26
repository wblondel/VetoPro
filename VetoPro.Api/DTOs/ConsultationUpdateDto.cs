using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une consultation existante.
/// </summary>
public class ConsultationUpdateDto
{
    /// <summary>
    /// ID du docteur qui effectue la consultation.
    /// </summary>
    [Required(ErrorMessage = "L'ID du docteur est obligatoire.")]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// Date et heure réelles de la consultation.
    /// </summary>
    [Required]
    public DateTime ConsultationDate { get; set; }

    // --- Constantes (Vitals) ---
    [Range(0, 500)]
    public decimal? WeightKg { get; set; }
        
    [Range(30, 45)]
    public decimal? TemperatureCelsius { get; set; }

    // --- Compte-rendu Clinique ---
    public string? ClinicalExam { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescriptions { get; set; }
}