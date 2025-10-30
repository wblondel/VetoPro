using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Entities;
using VetoPro.Api.Helpers;
using VetoPro.Api.Mapping;
using VetoPro.Contracts.DTOs;
using VetoPro.Contracts.DTOs.Catalogs;
using FluentValidation;

namespace VetoPro.Api.Controllers.Catalogs;

[Authorize]
public class BreedsController : BaseApiController
{
    private readonly IValidator<BreedCreateDto> _breedCreateValidator;
    private readonly IValidator<BreedUpdateDto> _breedUpdateValidator;
    
    public BreedsController(
        VetoProDbContext context,
        IValidator<BreedCreateDto> breedCreateValidator,
        IValidator<BreedUpdateDto> breedUpdateValidator) : base(context)
    {
        _breedCreateValidator = breedCreateValidator;
        _breedUpdateValidator = breedUpdateValidator;
    }

    /// <summary>
    /// GET: api/breeds
    /// Récupère la liste de toutes les races, avec le nom de leur espèce.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BreedDto>>> GetAllBreeds([FromQuery] PaginationParams paginationParams)
    {
        var query = _context.Breeds
            .Include(b => b.Species) // Jointure pour charger l'entité Species
            .OrderBy(b => b.Species.Name).ThenBy(b => b.Name) // Trier par espèce, puis par nom
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, b => b.ToDto());
    }

    /// <summary>
    /// GET: api/breeds/{id}
    /// Récupère une race spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BreedDto>> GetBreedById(Guid id)
    {
        var breed = await _context.Breeds
            .Include(b => b.Species)
            .Select(b => new BreedDto
            {
                Id = b.Id,
                Name = b.Name,
                SpeciesId = b.SpeciesId,
                SpeciesName = b.Species.Name
            })
            .FirstOrDefaultAsync(b => b.Id == id);

        if (breed == null)
        {
            return NotFound("La race avec cet ID n'a pas été trouvée.");
        }

        return Ok(breed);
    }

    /// <summary>
    /// POST: api/breeds
    /// Crée une nouvelle race.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<BreedDto>> CreateBreed([FromBody] BreedCreateDto createDto)
    {
        var validationResult = await _breedCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // 1. Vérifier si l'espèce parente existe
        var species = await _context.Species.FindAsync(createDto.SpeciesId);
        if (species == null)
        {
            return BadRequest("L'espèce (SpeciesId) fournie n'existe pas.");
        }

        // 2. Vérifier si le nom existe déjà *pour cette espèce*
        if (await _context.Breeds.AnyAsync(b => b.Name == createDto.Name && b.SpeciesId == createDto.SpeciesId))
        {
            return Conflict("Une race avec ce nom existe déjà pour cette espèce.");
        }

        // 3. Mapper et créer l'entité
        var newBreed = new Breed
        {
            Name = createDto.Name,
            SpeciesId = createDto.SpeciesId
        };

        _context.Breeds.Add(newBreed);
        await _context.SaveChangesAsync();

        // 4. Mapper au DTO de retour
        var breedDto = new BreedDto
        {
            Id = newBreed.Id,
            Name = newBreed.Name,
            SpeciesId = newBreed.SpeciesId,
            SpeciesName = species.Name // Nous avons déjà l'objet 'species'
        };

        return CreatedAtAction(nameof(GetBreedById), new { id = breedDto.Id }, breedDto);
    }

    /// <summary>
    /// PUT: api/breeds/{id}
    /// Met à jour une race existante.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdateBreed(Guid id, [FromBody] BreedUpdateDto updateDto)
    {
        var validationResult = await _breedUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        var breedToUpdate = await _context.Breeds.FindAsync(id);

        if (breedToUpdate == null)
        {
            return NotFound("La race à mettre à jour n'a pas été trouvée.");
        }

        // 1. Vérifier si la *nouvelle* espèce parente existe
        if (breedToUpdate.SpeciesId != updateDto.SpeciesId)
        {
            if (!await _context.Species.AnyAsync(s => s.Id == updateDto.SpeciesId))
            {
                return BadRequest("La nouvelle espèce (SpeciesId) fournie n'existe pas.");
            }
        }
        
        // 2. Vérifier les conflits de nom
        if (await _context.Breeds.AnyAsync(b => b.Name == updateDto.Name && b.SpeciesId == updateDto.SpeciesId && b.Id != id))
        {
            return Conflict("Une autre race avec ce nom existe déjà pour cette espèce.");
        }

        // 3. Appliquer les modifications
        breedToUpdate.Name = updateDto.Name;
        breedToUpdate.SpeciesId = updateDto.SpeciesId;

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/breeds/{id}
    /// Supprime une race.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBreed(Guid id)
    {
        var breedToDelete = await _context.Breeds.FindAsync(id);

        if (breedToDelete == null)
        {
            return NotFound("La race à supprimer n'a pas été trouvée.");
        }

        // Sécurité : Vérifier si la race est utilisée par des patients
        var isUsed = await _context.Entry(breedToDelete)
            .Collection(b => b.Patients)
            .Query()
            .AnyAsync();

        if (isUsed)
        {
            return BadRequest("Cette race ne peut pas être supprimée car elle est utilisée par un ou plusieurs patients.");
        }

        _context.Breeds.Remove(breedToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}