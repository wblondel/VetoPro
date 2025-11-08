namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour l'affichage complet d'une facture.
/// </summary>
public class InvoiceDto
{
    public Guid Id { get; set; }

    public string InvoiceNumber { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; }

    // --- Informations sur le Client ---
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } // Prénom + Nom

    // --- Informations sur la Consultation (Optionnel) ---
    public Guid? ConsultationId { get; set; }

    // --- Lignes de Facture ---
    public ICollection<InvoiceLineDto> InvoiceLines { get; set; } = new List<InvoiceLineDto>();
    
    // --- Paiements (Optionnel, juste le total perçu) ---
    public decimal AmountPaid { get; set; }
}