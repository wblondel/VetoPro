using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour la création (l'enregistrement) d'un nouveau paiement.
/// </summary>
public class PaymentCreateDto
{
    public Guid InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } // ex: "Card", "Cash", "Transfer"
    public string? TransactionId { get; set; }
}