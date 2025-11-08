namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour l'affichage d'une race, incluant les informations de son espèce.
/// </summary>
public class BreedDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// ID de l'espèce parente.
    /// </summary>
    public Guid SpeciesId { get; set; }

    /// <summary>
    /// Nom de l'espèce parente (pratique pour l'affichage).
    /// </summary>
    public string SpeciesName { get; set; }
}