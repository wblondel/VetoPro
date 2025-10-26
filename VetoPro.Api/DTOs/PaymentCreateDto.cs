using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la création (l'enregistrement) d'un nouveau paiement.
/// </summary>
public class PaymentCreateDto
{
    [Required(ErrorMessage = "L'ID de la facture (InvoiceId) est obligatoire.")]
    public Guid InvoiceId { get; set; }

    [Required(ErrorMessage = "La date du paiement est obligatoire.")]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Le montant est obligatoire.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Le montant doit être positif.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "La méthode de paiement est obligatoire.")]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } // ex: "Card", "Cash", "Transfer"

    [MaxLength(255)]
    public string? TransactionId { get; set; }
}