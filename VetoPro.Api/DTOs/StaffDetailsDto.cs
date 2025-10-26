using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour l'affichage des informations spécifiques au personnel.
/// Sera imbriqué dans ContactDto.
/// </summary>
public class StaffDetailsDto
{
    public Guid Id { get; set; }

    [Required]
    public string Role { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Specialty { get; set; }

    public bool IsActive { get; set; }
}