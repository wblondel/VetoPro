using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VetoPro.Contracts.DTOs;
using VetoPro.Contracts.DTOs.Catalogs;

namespace VetoPro.Services;

public interface ISpeciesService
{
    Task<List<SpeciesDto>> GetAllSpeciesAsync();
    Task<List<BreedDto>> GetBreedsForSpeciesAsync(Guid speciesId);
}

public class SpeciesService(HttpClient httpClient) : ApiServiceBase(httpClient), ISpeciesService
{
    /// <summary>
    /// Récupère la liste de toutes les espèces
    /// GET /api/species
    /// </summary>
    public async Task<List<SpeciesDto>> GetAllSpeciesAsync()
    {
        return await GetListAsync<SpeciesDto>("api/species");
    }

    /// <summary>
    /// Récupère la liste des races pour une espèce donnée
    /// GET /api/species/{id}/breeds
    /// </summary>
    public async Task<List<BreedDto>> GetBreedsForSpeciesAsync(Guid speciesId)
    {
        return await GetListAsync<BreedDto>($"api/species/{speciesId}/breeds");
    }
}