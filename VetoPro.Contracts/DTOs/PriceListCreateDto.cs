using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la création d'une nouvelle règle de prix.
/// </summary>
public class PriceListCreateDto
{
    [Required(ErrorMessage = "L'ID du service est obligatoire.")]
    public Guid ServiceId { get; set; }

    /// <summary>
    /// ID de l'espèce (optionnel).
    /// Si null, s'applique à toutes les espèces.
    /// </summary>
    public Guid? SpeciesId { get; set; }

    [Range(0, 500, ErrorMessage = "Le poids minimum doit être valide.")]
    public decimal? WeightMinKg { get; set; }

    [Range(0, 500, ErrorMessage = "Le poids maximum doit être valide.")]
    public decimal? WeightMaxKg { get; set; }

    [Required(ErrorMessage = "Le montant est obligatoire.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Le montant ne peut pas être négatif.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "La devise est obligatoire.")]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR"; // Valeur par défaut

    public bool IsActive { get; set; } = true;
}