using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente une espèce (ex: "Chien", "Chat", "Cheval").
/// C'est le parent des races (Breeds).
/// </summary>
public class Species : BaseEntity
{
    /// <summary>
    /// Nom de l'espèce (doit être unique).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Liste des races appartenant à cette espèce.
    /// </summary>
    public ICollection<Breed> Breeds { get; set; } = new List<Breed>();

    /// <summary>
    /// Liste des règles de prix s'appliquant spécifiquement à cette espèce.
    /// </summary>
    public ICollection<PriceList> PriceRules { get; set; } = new List<PriceList>();
}