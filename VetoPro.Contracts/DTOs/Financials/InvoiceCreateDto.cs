using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour la création d'une nouvelle facture (Invoice).
/// Inclut la liste des lignes de détail (Invoice Lines).
/// </summary>
public class InvoiceCreateDto
{
    public Guid ClientId { get; set; }
    public Guid? ConsultationId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "Draft"; // Valeur par défaut
    public ICollection<InvoiceLineCreateDto> InvoiceLines { get; set; } = new List<InvoiceLineCreateDto>();
}