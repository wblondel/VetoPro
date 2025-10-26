using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une couleur existante.
/// </summary>
public class ColorUpdateDto
{
    [Required(ErrorMessage = "Le nom de la couleur est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; }

    [MaxLength(10, ErrorMessage = "La valeur hexadécimale ne peut pas dépasser 10 caractères.")]
    public string? HexValue { get; set; }
}