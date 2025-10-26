using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class ColorMapper
{
    /// <summary>
    /// Maps a Color entity to a ColorDto.
    /// </summary>
    public static ColorDto ToDto(this Color c)
    {
        return new ColorDto
        {
            Id = c.Id,
            Name = c.Name,
            HexValue = c.HexValue
        };
    }
}