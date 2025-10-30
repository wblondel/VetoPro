using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la création d'une nouvelle couleur.
/// Ne contient que les champs modifiables par l'utilisateur.
/// </summary>
public class ColorCreateDto
{
    public string Name { get; set; }
    public string? HexValue { get; set; }
}