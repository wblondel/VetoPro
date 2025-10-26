using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente une couleur ou un motif de robe (ex: "Noir", "Tigré", "Tricolore").
/// </summary>
public class Color : BaseEntity
{
    /// <summary>
    /// Nom de la couleur (doit être unique).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// Valeur hexadécimale (optionnelle) représentant la couleur (ex: "#FF0000").
    /// </summary>
    [MaxLength(10)] // Assez pour "#RRGGBBAA"
    public string? HexValue { get; set; }


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Liste des patients ayant cette couleur (relation plusieurs-à-plusieurs).
    /// </summary>
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}