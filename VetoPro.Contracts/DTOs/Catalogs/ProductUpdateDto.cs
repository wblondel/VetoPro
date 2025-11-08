namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un produit (article physique) existant.
/// </summary>
public class ProductUpdateDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int StockQuantity { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}