using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

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

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; }

    public string? TransactionId { get; set; }
}