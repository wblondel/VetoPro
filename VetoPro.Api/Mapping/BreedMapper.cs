using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using VetoPro.Contracts.DTOs.Catalogs;

namespace VetoPro.Api.Mapping;

public static class BreedMapper
{
    /// <summary>
    /// Maps a Breed entity to a BreedDto.
    /// Assumes the related Species entity is loaded.
    /// </summary>
    public static BreedDto ToDto(this Breed b)
    {
        // Handle potential null Species (though FK constraint makes it unlikely)
        var speciesName = (b.Species != null) ? b.Species.Name : "N/A";
        var speciesId = (b.Species != null) ? b.Species.Id : Guid.Empty;

        return new BreedDto
        {
            Id = b.Id,
            Name = b.Name,
            SpeciesId = speciesId,
            SpeciesName = speciesName
        };
    }
    
    /// <summary>
    /// Surcharge optimisée pour le mapper BreedDto.
    /// Utilisé lorsque le nom de l'espèce est déjà connu et n'a pas besoin d'être chargé.
    /// </summary>
    public static BreedDto ToDto(this Breed b, string speciesName)
    {
        return new BreedDto
        {
            Id = b.Id,
            Name = b.Name,
            SpeciesId = b.SpeciesId, // SpeciesId est directement sur l'entité Breed
            SpeciesName = speciesName // Utilise le nom fourni
        };
    }
}