using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une race existante.
/// </summary>
public class BreedUpdateDto
{
    [Required(ErrorMessage = "Le nom de la race est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; }

    /// <summary>
    /// ID de l'espèce parente (ex: l'ID de "Chien").
    /// </summary>
    [Required(ErrorMessage = "L'ID de l'espèce est obligatoire.")]
    public Guid SpeciesId { get; set; }
}