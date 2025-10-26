using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente un paiement reçu pour une facture.
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers la facture concernée par ce paiement.
    /// </summary>
    [Required]
    [ForeignKey("Invoice")]
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } // Propriété de navigation

    /// <summary>
    /// Date et heure (UTC) de la réception du paiement.
    /// </summary>
    [Required]
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Montant du paiement reçu.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Méthode de paiement (ex: "Card", "Cash", "Transfer").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; }

    /// <summary>
    /// Identifiant de la transaction (optionnel, pour les paiements par carte/virement).
    /// </summary>
    [MaxLength(255)]
    public string? TransactionId { get; set; }
}