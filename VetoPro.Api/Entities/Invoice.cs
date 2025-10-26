using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente une facture adressée à un client.
/// </summary>
public class Invoice : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers le client qui doit payer la facture.
    /// </summary>
    [Required]
    [ForeignKey("Client")]
    public Guid ClientId { get; set; }
    public Contact Client { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (optionnelle) vers la consultation qui a généré cette facture.
    /// </summary>
    [ForeignKey("Consultation")]
    public Guid? ConsultationId { get; set; }
    public Consultation? Consultation { get; set; } // Propriété de navigation

    /// <summary>
    /// Numéro de facture unique (ex: "F-2025-0001").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; }

    /// <summary>
    /// Date d'émission de la facture.
    /// </summary>
    [Required]
    public DateOnly IssueDate { get; set; }

    /// <summary>
    /// Date d'échéance du paiement.
    /// </summary>
    [Required]
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Montant total de la facture (calculé à partir des lignes).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Statut de la facture (ex: "Draft", "Sent", "Paid", "Void").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; }


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Liste des lignes de détail de la facture.
    /// </summary>
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();

    /// <summary>
    /// Liste des paiements reçus pour cette facture.
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}