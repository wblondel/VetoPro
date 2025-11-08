namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour l'affichage d'un paiement reçu.
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la facture (Invoice) à laquelle ce paiement est lié.
    /// </summary>
    public Guid InvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; }

    public string? TransactionId { get; set; }
}