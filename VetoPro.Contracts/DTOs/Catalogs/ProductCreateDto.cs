namespace VetoPro.Contracts.DTOs.Catalogs;

/// <summary>
/// DTO pour la création d'un nouveau produit (article physique).
/// </summary>
public class ProductCreateDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int StockQuantity { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}