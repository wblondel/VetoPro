using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une ligne de détail sur une facture.
/// Sera imbriqué dans InvoiceUpdateDto.
/// </summary>
public class InvoiceLineUpdateDto
{
    /// <summary>
    /// ID de la ligne de facture existante (optionnel).
    /// Si null, cette ligne sera traitée comme une nouvelle addition.
    /// </summary>
    public Guid? Id { get; set; }

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

    [Required(ErrorMessage = "Le prix unitaire est obligatoire.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Le prix ne peut pas être négatif.")]
    public decimal UnitPrice { get; set; }
}