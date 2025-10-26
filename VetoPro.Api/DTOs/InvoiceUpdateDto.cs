using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'une facture existante.
/// La liste des lignes (InvoiceLines) fournie remplacera
/// entièrement la liste existante sur la facture.
/// </summary>
public class InvoiceUpdateDto
{
    [Required(ErrorMessage = "L'ID du client (ClientId) est obligatoire.")]
    public Guid ClientId { get; set; }

    /// <summary>
    /// ID optionnel de la consultation qui a généré cette facture.
    /// </summary>
    public Guid? ConsultationId { get; set; }

    [Required(ErrorMessage = "Le numéro de facture (InvoiceNumber) est obligatoire.")]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; }

    [Required(ErrorMessage = "La date d'émission (IssueDate) est obligatoire.")]
    public DateOnly IssueDate { get; set; }

    [Required(ErrorMessage = "La date d'échéance (DueDate) est obligatoire.")]
    public DateOnly DueDate { get; set; }

    [Required(ErrorMessage = "Le statut (Status) est obligatoire.")]
    [MaxLength(50)]
    public string Status { get; set; }

    /// <summary>
    /// Liste *complète* des lignes de détail de la facture.
    /// Doit contenir au moins une ligne.
    /// L'API synchronisera la base de données avec cette liste.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "La facture doit contenir au moins une ligne.")]
    public ICollection<InvoiceLineUpdateDto> InvoiceLines { get; set; } = new List<InvoiceLineUpdateDto>();
}