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

        return new BreedDto
        {
            Id = b.Id,
            Name = b.Name,
            SpeciesId = b.SpeciesId,
            SpeciesName = speciesName
        };
    }
}