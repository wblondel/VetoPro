namespace VetoPro.Contracts.DTOs.Management;

/// <summary>
/// DTO pour la *mise à jour* des détails du personnel.
/// Sera imbriqué dans ContactUpdateDto.
/// </summary>
public class StaffDetailsUpdateDto
{
    public string Role { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Specialty { get; set; }
    public bool IsActive { get; set; } = true;
}