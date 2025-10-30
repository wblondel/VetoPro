using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un service ou acte existant.
/// </summary>
public class ServiceUpdateDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresPatient { get; set; } = true;
}