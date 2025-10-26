using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la *création* de détails du personnel.
/// Sera imbriqué dans ContactCreateDto.
/// </summary>
public class StaffDetailsCreateDto
{
    [Required(ErrorMessage = "Le rôle est obligatoire.")]
    [MaxLength(100)]
    public string Role { get; set; }

    [MaxLength(100)]
    public string? LicenseNumber { get; set; }

    [MaxLength(100)]
    public string? Specialty { get; set; }

    public bool IsActive { get; set; } = true;
}