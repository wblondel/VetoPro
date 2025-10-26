using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente un produit physique vendable (médicament, nourriture, accessoire).
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Nom du produit (doit être unique).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// Description détaillée du produit (optionnelle).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Quantité actuellement en stock.
    /// </summary>
    public int StockQuantity { get; set; } = 0;

    /// <summary>
    /// Prix de vente unitaire.
    /// (Il faut définir si c'est HT ou TTC).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Indique si ce produit est actuellement en vente.
    /// </summary>
    public bool IsActive { get; set; } = true;


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Lignes de facture faisant référence à ce produit.
    /// </summary>
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}