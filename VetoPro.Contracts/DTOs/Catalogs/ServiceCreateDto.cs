using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la création d'un nouveau service ou acte.
/// </summary>
public class ServiceCreateDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresPatient { get; set; } = true;
}