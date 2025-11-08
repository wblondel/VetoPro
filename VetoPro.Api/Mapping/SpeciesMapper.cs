using VetoPro.Contracts.DTOs.Catalogs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class SpeciesMapper
{
    /// <summary>
    /// Mappe une entité Species vers un SpeciesDto.
    /// </summary>
    public static SpeciesDto ToDto(this Species s)
    {
        return new SpeciesDto
        {
            Id = s.Id,
            Name = s.Name
        };
    }
}