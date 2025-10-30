namespace VetoPro.Contracts.DTOs.Management;

/// <summary>
/// DTO pour la *création* de détails du personnel.
/// Sera imbriqué dans ContactCreateDto.
/// </summary>
public class StaffDetailsCreateDto
{
    public string Role { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Specialty { get; set; }
    public bool IsActive { get; set; } = true;
}