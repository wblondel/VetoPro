using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using VetoPro.Contracts.DTOs.Catalogs;
using VetoPro.Contracts.DTOs.Clinical;

namespace VetoPro.Api.Mapping;

public static class PatientMapper
{
    /// <summary>
    /// Maps a Patient entity to a PatientDto.
    /// Assumes related entities (Owner, Breed.Species, Colors) are loaded.
    /// </summary>
    public static PatientDto ToDto(this Patient p)
    {
        // Handle cases where navigation properties might theoretically be null
        var ownerName = (p.Owner != null) ? $"{p.Owner.FirstName} {p.Owner.LastName}" : "N/A";
        var breedName = (p.Breed != null) ? p.Breed.Name : "N/A";
        var speciesName = (p.Breed?.Species != null) ? p.Breed.Species.Name : "N/A"; // Chain null checks

        return new PatientDto
        {
            Id = p.Id,
            Name = p.Name,
            ChipNumber = p.ChipNumber,
            DobEstimateStart = p.DobEstimateStart,
            DobEstimateEnd = p.DobEstimateEnd,
            Gender = p.Gender,
            ReproductiveStatus = p.ReproductiveStatus,
            DeceasedAt = p.DeceasedAt,

            // Map Owner
            OwnerId = p.OwnerId,
            OwnerFullName = ownerName,

            // Map Breed
            BreedId = p.BreedId,
            BreedName = breedName,

            // Map Species (via Breed)
            SpeciesId = p.Breed?.SpeciesId ?? Guid.Empty, // Handle potential null Breed
            SpeciesName = speciesName,

            // Map Colors (M2M)
            Colors = p.Colors?.Select(c => new ColorDto // Handle potential null Colors collection
            {
                Id = c.Id,
                Name = c.Name,
                HexValue = c.HexValue
            }).ToList() ?? new List<ColorDto>() // Return empty list if Colors is null
        };
    }
}