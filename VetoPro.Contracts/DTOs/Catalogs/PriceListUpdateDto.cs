namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une règle de prix existante.
/// </summary>
public class PriceListUpdateDto
{
    public Guid ServiceId { get; set; }
    public Guid? SpeciesId { get; set; }
    public decimal? WeightMinKg { get; set; }
    public decimal? WeightMaxKg { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; } = true;
}