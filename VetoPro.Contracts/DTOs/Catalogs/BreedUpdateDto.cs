namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une race existante.
/// </summary>
public class BreedUpdateDto
{
    public string Name { get; set; }
    public Guid SpeciesId { get; set; }
}