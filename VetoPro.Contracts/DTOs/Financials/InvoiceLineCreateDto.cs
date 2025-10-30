using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour la création d'une ligne de détail sur une facture.
/// Sera imbriqué dans InvoiceCreateDto.
/// </summary>
public class InvoiceLineCreateDto
{
    public Guid ItemId { get; set; }
    public string ItemType { get; set; } // "Service" ou "Product"
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}