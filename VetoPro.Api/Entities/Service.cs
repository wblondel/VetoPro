using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente un service ou un acte médical facturable (ex: "Consultation", "Stérilisation").
/// </summary>
public class Service : BaseEntity
{
    /// <summary>
    /// Nom du service (doit être unique).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// Description détaillée du service (optionnelle).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indique si ce service est actuellement proposé.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indique si ce service nécessite un patient (ex: une consultation)
    /// ou peut être vendu "au comptoir" (ex: "Frais de dossier").
    /// </summary>
    public bool RequiresPatient { get; set; } = true;


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Liste des règles de prix associées à ce service.
    /// </summary>
    public ICollection<PriceList> PriceRules { get; set; } = new List<PriceList>();

    /// <summary>
    /// Lignes de facture faisant référence à ce service.
    /// </summary>
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
}