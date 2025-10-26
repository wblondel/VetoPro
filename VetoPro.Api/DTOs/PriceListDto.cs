using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour l'affichage d'une règle de prix.
/// </summary>
public class PriceListDto
{
    public Guid Id { get; set; }

    [Required]
    public Guid ServiceId { get; set; }
    [Required]
    public string ServiceName { get; set; }

    // L'espèce est optionnelle.
    public Guid? SpeciesId { get; set; }
    public string? SpeciesName { get; set; } // Null si la règle s'applique à tous

    public decimal? WeightMinKg { get; set; }
    public decimal? WeightMaxKg { get; set; }

    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string Currency { get; set; }
    
    public bool IsActive { get; set; }
}