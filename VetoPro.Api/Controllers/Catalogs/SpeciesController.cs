using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Mapping;
using VetoPro.Contracts.DTOs;
using VetoPro.Contracts.DTOs.Catalogs;

namespace VetoPro.Api.Controllers.Catalogs;

/// <summary>
/// API pour la gestion du catalogue des espèces animales.
/// Permet de lire les espèces et les races associées.
/// </summary>
[Authorize]
public class SpeciesController(VetoProDbContext context) : BaseApiController(context)
{
    /// <summary>
    /// Récupère la liste de toutes les espèces.
    /// </summary>
    /// <returns>Une liste complète d'espèces.</returns>
    [HttpGet] // Répond à la requête : GET /api/species
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SpeciesDto>>> GetSpecies()
    {
        var speciesList = await Context.Species
            .OrderBy(s => s.Name)
            .Select(s => s.ToDto())
            .ToListAsync();
        
        return Ok(speciesList);
    }
    
    /// <summary>
    /// Récupère la liste de toutes les races pour une espèce donnée.
    /// </summary>
    /// <param name="id">L'ID (UUID) de l'espèce.</param>
    /// <returns>Une liste de races.</returns>
    [HttpGet("{id:guid}/breeds")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BreedDto>>> GetBreedsForSpecies(Guid id)
    {
        // 1. Vérifier si l'espèce parente existe
        var speciesName = await Context.Species
            .Where(s => s.Id == id)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();
        
        if (speciesName == null)
        {
            return NotFound("L'espèce avec cet ID n'a pas été trouvée.");
        }

        // 2. Récupérer les races liées
        var breeds = await Context.Breeds
            .Where(b => b.SpeciesId == id) // Filtrer par SpeciesId
            .OrderBy(b => b.Name)
            .Select(b => b.ToDto(speciesName))
            .ToListAsync();

        return Ok(breeds);
    }
}