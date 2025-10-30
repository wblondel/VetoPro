using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour la création d'une consultation (compte-rendu médical).
/// Les IDs (Patient, Client, Docteur) sont gérés par le contrôleur.
/// </summary>
public class ConsultationCreateDto
{ 
    public Guid DoctorId { get; set; }
    public DateTime ConsultationDate { get; set; } = DateTime.UtcNow;
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string? ClinicalExam { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Prescriptions { get; set; }
}