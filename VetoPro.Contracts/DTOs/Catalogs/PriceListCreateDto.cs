namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la création d'une nouvelle règle de prix.
/// </summary>
public class PriceListCreateDto
{
    public Guid ServiceId { get; set; }
    public Guid? SpeciesId { get; set; }
    public decimal? WeightMinKg { get; set; }
    public decimal? WeightMaxKg { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR"; // Valeur par défaut
    public bool IsActive { get; set; } = true;
}