using VetoPro.Contracts.DTOs.Catalogs;

namespace VetoPro.Contracts.DTOs.Clinical;

/// <summary>
/// DTO pour l'affichage complet d'un patient.
/// Inclut les informations sur le propriétaire, la race, l'espèce et les couleurs.
/// </summary>
public class PatientDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? ChipNumber { get; set; }

    public DateOnly DobEstimateStart { get; set; }

    public DateOnly DobEstimateEnd { get; set; }

    public string Gender { get; set; }

    public string ReproductiveStatus { get; set; }
    
    public DateTime? DeceasedAt { get; set; }

    // --- Informations sur le Propriétaire (Owner) ---
    public Guid OwnerId { get; set; }
    public string OwnerFullName { get; set; } // Prénom + Nom

    // --- Informations sur la Race (Breed) ---
    public Guid BreedId { get; set; }
    public string BreedName { get; set; }

    // --- Informations sur l'Espèce (Species) ---
    public Guid SpeciesId { get; set; }
    public string SpeciesName { get; set; }

    // --- Informations sur les Couleurs (Colors) ---
    public ICollection<ColorDto> Colors { get; set; } = new List<ColorDto>();
}