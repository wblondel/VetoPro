using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente une ligne de détail sur une facture.
/// Peut être un service (acte) ou un produit (article).
/// </summary>
public class InvoiceLine : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers la facture parente.
    /// </summary>
    [Required]
    [ForeignKey("Invoice")]
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (optionnelle) vers le service facturé.
    /// Null si la ligne concerne un produit.
    /// </summary>
    [ForeignKey("Service")]
    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (optionnelle) vers le produit facturé.
    /// Null si la ligne concerne un service.
    /// </summary>
    [ForeignKey("Product")]
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; } // Propriété de navigation

    /// <summary>
    /// Description de la ligne (copiée depuis le service/produit au moment
    /// de la facturation, pour l'immuabilité).
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Description { get; set; }

    /// <summary>
    /// Quantité facturée.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Prix unitaire au moment de la facturation.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total de la ligne (Quantité * PrixUnitaire).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal LineTotal { get; set; }
}