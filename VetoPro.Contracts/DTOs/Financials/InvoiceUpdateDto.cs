namespace VetoPro.Contracts.DTOs.Financials;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une facture existante.
/// La liste des lignes (InvoiceLines) fournie remplacera
/// entièrement la liste existante sur la facture.
/// </summary>
public class InvoiceUpdateDto
{
    public Guid ClientId { get; set; }
    public Guid? ConsultationId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; }
    public ICollection<InvoiceLineUpdateDto> InvoiceLines { get; set; } = new List<InvoiceLineUpdateDto>();
}