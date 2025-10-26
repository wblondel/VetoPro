using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la *mise à jour* des détails du personnel.
/// Sera imbriqué dans ContactUpdateDto.
/// </summary>
public class StaffDetailsUpdateDto
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