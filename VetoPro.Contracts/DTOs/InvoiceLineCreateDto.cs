using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la création d'une ligne de détail sur une facture.
/// Sera imbriqué dans InvoiceCreateDto.
/// </summary>
public class InvoiceLineCreateDto
{
    /// <summary>
    /// ID du Service ou du Produit à facturer.
    /// </summary>
    [Required(ErrorMessage = "L'ID de l'article (ItemId) est obligatoire.")]
    public Guid ItemId { get; set; }

    /// <summary>
    /// Type d'article : "Service" ou "Product".
    /// </summary>
    [Required(ErrorMessage = "Le type d'article (ItemType) est obligatoire.")]
    public string ItemType { get; set; } // "Service" ou "Product"

    [Required(ErrorMessage = "La quantité est obligatoire.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "La quantité doit être positive.")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Prix unitaire au moment de la facturation.
    /// L'API doit le valider ou le récupérer depuis le catalogue.
    /// </summary>
    [Required(ErrorMessage = "Le prix unitaire est obligatoire.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Le prix ne peut pas être négatif.")]
    public decimal UnitPrice { get; set; }
}