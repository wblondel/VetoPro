using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Représente une race spécifique (ex: "Berger Allemand", "Siamois").
/// Chaque race est obligatoirement liée à une espèce.
/// </summary>
public class Breed : BaseEntity
{
    /// <summary>
    /// Clé étrangère (obligatoire) vers l'espèce.
    /// </summary>
    [Required]
    [ForeignKey("Species")]
    public Guid SpeciesId { get; set; }
    public Species Species { get; set; } // Propriété de navigation

    /// <summary>
    /// Nom de la race (ex: "Labrador Retriever").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }


    // --- Propriétés de Navigation ---

    /// <summary>
    /// Liste des patients de cette race.
    /// </summary>
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}