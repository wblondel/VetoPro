namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// Représente une espèce animale (ex: "Chien", "Chat").
/// </summary>
public class SpeciesDto
{
    /// <summary>
    /// L'identifiant unique (UUID) de l'espèce.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Le nom de l'espèce.
    /// </summary>
    public string Name { get; set; }
}