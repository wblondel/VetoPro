using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour l'affichage d'une couleur.
/// </summary>
public class ColorDto
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Valeur hexadécimale (optionnelle) représentant la couleur (ex : "#FF0000").
    /// </summary>
    public string? HexValue { get; set; }
}