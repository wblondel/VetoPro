namespace VetoPro.Contracts.DTOs.Management;

/// <summary>
/// DTO pour l'affichage des informations spécifiques au personnel.
/// Sera imbriqué dans ContactDto.
/// </summary>
public class StaffDetailsDto
{
    public Guid Id { get; set; }

    public string Role { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Specialty { get; set; }

    public bool IsActive { get; set; }
}