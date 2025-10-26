using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour l'affichage complet d'une facture.
/// </summary>
public class InvoiceDto
{
    public Guid Id { get; set; }

    [Required]
    public string InvoiceNumber { get; set; }

    [Required]
    public DateOnly IssueDate { get; set; }

    [Required]
    public DateOnly DueDate { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public string Status { get; set; }

    // --- Informations sur le Client ---
    public Guid ClientId { get; set; }
    [Required]
    public string ClientName { get; set; } // Prénom + Nom

    // --- Informations sur la Consultation (Optionnel) ---
    public Guid? ConsultationId { get; set; }

    // --- Lignes de Facture ---
    public ICollection<InvoiceLineDto> InvoiceLines { get; set; } = new List<InvoiceLineDto>();
    
    // --- Paiements (Optionnel, juste le total perçu) ---
    public decimal AmountPaid { get; set; }
}