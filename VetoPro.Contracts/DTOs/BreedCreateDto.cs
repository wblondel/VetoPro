using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la création d'une nouvelle race (Breed).
/// </summary>
public class BreedCreateDto
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