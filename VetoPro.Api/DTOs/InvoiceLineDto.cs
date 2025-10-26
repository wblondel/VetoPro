namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour l'affichage d'une ligne de détail sur une facture.
/// </summary>
public class InvoiceLineDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID du service (si c'est un acte) ou du produit (si c'est un article).
    /// </summary>
    public Guid? ItemId { get; set; } // ServiceId ou ProductId

    /// <summary>
    /// "Service" ou "Product".
    /// </summary>
    public string ItemType { get; set; }

    /// <summary>
    /// Description de la ligne (ex: "Vaccin CHPPi", "Sac de croquettes 3kg").
    /// </summary>
    public string Description { get; set; }

    public decimal Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal LineTotal { get; set; }
}