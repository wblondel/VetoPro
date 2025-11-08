namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une ligne de détail sur une facture.
/// Sera imbriqué dans InvoiceUpdateDto.
/// </summary>
public class InvoiceLineUpdateDto
{
    public Guid? Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemType { get; set; } // "Service" ou "Product"
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}