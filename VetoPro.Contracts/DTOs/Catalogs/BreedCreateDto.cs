using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la création d'une nouvelle race (Breed).
/// </summary>
public class BreedCreateDto
{
    public string Name { get; set; }
    public Guid SpeciesId { get; set; }
}