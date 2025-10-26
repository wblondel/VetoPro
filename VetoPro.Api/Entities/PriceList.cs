using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Définit une règle de prix pour un service.
/// Le prix peut dépendre de l'espèce et/ou d'une tranche de poids.
/// </summary>
public class PriceList : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers le service concerné.
    /// </summary>
    [Required]
    [ForeignKey("Service")]
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } // Propriété de navigation

    /// <summary>
    /// Clé étrangère (optionnelle) vers l'espèce.
    /// Si null, cette règle de prix s'applique à toutes les espèces.
    /// </summary>
    [ForeignKey("Species")]
    public Guid? SpeciesId { get; set; }
    public Species? Species { get; set; } // Propriété de navigation

    /// <summary>
    /// Poids minimum (kg) pour que cette règle s'applique (optionnel).
    /// </summary>
    [Column(TypeName = "decimal(6, 2)")]
    public decimal? WeightMinKg { get; set; }

    /// <summary>
    /// Poids maximum (kg) pour que cette règle s'applique (optionnel).
    /// </summary>
    [Column(TypeName = "decimal(6, 2)")]
    public decimal? WeightMaxKg { get; set; }

    /// <summary>
    /// Le montant du prix pour cette règle.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise du prix (ex: "EUR", "USD").
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; }

    /// <summary>
    /// Indique si cette règle de prix est actuellement active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}