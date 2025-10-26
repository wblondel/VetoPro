using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class ServiceMapper
{
    /// <summary>
    /// Mappe une entité Service vers un ServiceDto.
    /// </summary>
    public static ServiceDto ToDto(this Service s)
    {
        return new ServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsActive = s.IsActive,
            RequiresPatient = s.RequiresPatient
        };
    }
}