using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class ProductMapper
{
    /// <summary>
    /// Maps a Product entity to a ProductDto.
    /// </summary>
    public static ProductDto ToDto(this Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            StockQuantity = p.StockQuantity,
            UnitPrice = p.UnitPrice,
            IsActive = p.IsActive
        };
    }
}