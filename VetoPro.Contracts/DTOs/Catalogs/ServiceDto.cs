namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour l'affichage d'un service ou acte médical.
/// </summary>
public class ServiceDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Indique si ce service nécessite un patient (true)
    /// ou peut être vendu "au comptoir" (false).
    /// </summary>
    public bool RequiresPatient { get; set; }
}