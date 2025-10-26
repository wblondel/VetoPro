using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;

namespace VetoPro.Api.Controllers;

/// <summary>
/// API Controller pour la gestion des Espèces (Species).
/// </summary>
[ApiController]
[Route("api/[controller]")] // Définit la route de base : "api/species"
public class SpeciesController : ControllerBase
{
    private readonly VetoProDbContext _context;

    // Le DbContext est injecté par .NET (Dependency Injection)
    public SpeciesController(VetoProDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère la liste de toutes les espèces.
    /// </summary>
    [HttpGet] // Répond à la requête : GET /api/species
    public async Task<ActionResult<IEnumerable<SpeciesDto>>> GetSpecies()
    {
        try
        {
            var speciesList = await _context.Species
                .OrderBy(s => s.Name) // Toujours bon de trier
                .Select(s => new SpeciesDto // Conversion de l'Entité en DTO
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();

            // Renvoie une réponse 200 OK avec la liste en JSON
            return Ok(speciesList);
        }
        catch (Exception ex)
        {
            // En cas d'erreur de base de données
            return StatusCode(500, "Erreur interne du serveur: " + ex.Message);
        }
    }
    
    /// <summary>
    /// GET: api/species/{id}/breeds
    /// Récupère la liste de toutes les races pour une espèce donnée.
    /// </summary>
    [HttpGet("{id}/breeds")] // La nouvelle route imbriquée
    public async Task<ActionResult<IEnumerable<BreedDto>>> GetBreedsForSpecies(Guid id)
    {
        // 1. Vérifier si l'espèce parente existe
        var species = await _context.Species.FindAsync(id);
        if (species == null)
        {
            return NotFound("L'espèce avec cet ID n'a pas été trouvée.");
        }

        // 2. Récupérer les races liées
        var breeds = await _context.Breeds
            .Where(b => b.SpeciesId == id) // Filtrer par SpeciesId
            .OrderBy(b => b.Name)
            .Select(b => new BreedDto
            {
                Id = b.Id,
                Name = b.Name,
                SpeciesId = b.SpeciesId,
                SpeciesName = species.Name // On utilise le nom de l'espèce déjà trouvée
            })
            .ToListAsync();

        return Ok(breeds);
    }
}