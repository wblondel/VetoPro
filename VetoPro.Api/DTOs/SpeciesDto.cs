using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour l'entité Species.
/// Ne contient que les informations publiques nécessaires au client.
/// </summary>
public class SpeciesDto
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
}