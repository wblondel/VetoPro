using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class PriceListMapper
{
    /// <summary>
    /// Mappe une entité PriceList vers un PriceListDto.
    /// S'attend à ce que les entités liées (Service, Species) soient chargées.
    /// </summary>
    public static PriceListDto ToDto(this PriceList pl)
    {
        // Gérer les cas où les propriétés de navigation pourraient être nulles
        var serviceName = (pl.Service != null) ? pl.Service.Name : "N/A";
        // Species est optionnel
        var speciesName = pl.Species?.Name; // Renvoie null si pl.Species est null

        return new PriceListDto
        {
            Id = pl.Id,
            ServiceId = pl.ServiceId,
            ServiceName = serviceName,
            SpeciesId = pl.SpeciesId,
            SpeciesName = speciesName, // Sera null si SpeciesId est null
            WeightMinKg = pl.WeightMinKg,
            WeightMaxKg = pl.WeightMaxKg,
            Amount = pl.Amount,
            Currency = pl.Currency,
            IsActive = pl.IsActive
        };
    }
}