using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour l'affichage d'un rendez-vous.
/// Inclut les noms du client, du patient et du docteur.
/// </summary>
public class AppointmentDto
{
    public Guid Id { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    [Required]
    public string Status { get; set; }

    // --- Informations sur le Client ---
    public Guid ClientId { get; set; }
    [Required]
    public string ClientName { get; set; } // Prénom + Nom

    // --- Informations sur le Patient ---
    public Guid PatientId { get; set; }
    [Required]
    public string PatientName { get; set; }

    // --- Informations sur le Docteur (Optionnel) ---
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; } // Prénom + Nom
}