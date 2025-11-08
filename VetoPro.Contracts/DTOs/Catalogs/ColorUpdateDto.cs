namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une couleur existante.
/// </summary>
public class ColorUpdateDto
{
    public string Name { get; set; }
    public string? HexValue { get; set; }
}