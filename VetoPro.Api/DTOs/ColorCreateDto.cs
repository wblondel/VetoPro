using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la création d'une nouvelle couleur.
/// Ne contient que les champs modifiables par l'utilisateur.
/// </summary>
public class ColorCreateDto
{
    [Required(ErrorMessage = "Le nom de la couleur est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; }

    [MaxLength(10, ErrorMessage = "La valeur hexadécimale ne peut pas dépasser 10 caractères.")]
    public string? HexValue { get; set; }
}