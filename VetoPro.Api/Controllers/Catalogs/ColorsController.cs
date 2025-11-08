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

// Nécessaire pour l'entité 'Color'

namespace VetoPro.Api.Controllers.Catalogs;

[Authorize]
public class ColorsController(
    VetoProDbContext context,
    IValidator<ColorCreateDto> colorCreateValidator,
    IValidator<ColorUpdateDto> colorUpdateValidator)
    : BaseApiController(context)
{
    /// <summary>
    /// GET: api/colors
    /// Récupère la liste de toutes les couleurs.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ColorDto>>> GetAllColors([FromQuery] PaginationParams paginationParams)
    {
        var query = Context.Colors
            .OrderBy(c => c.Name)
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, c => c.ToDto());
    }

    /// <summary>
    /// GET: api/colors/{id}
    /// Récupère une couleur spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ColorDto>> GetColorById(Guid id)
    {
        var color = await Context.Colors
            .Select(c => new ColorDto
            {
                Id = c.Id,
                Name = c.Name,
                HexValue = c.HexValue
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (color == null)
        {
            return NotFound("La couleur avec cet ID n'a pas été trouvée.");
        }

        return Ok(color);
    }

    /// <summary>
    /// POST: api/colors
    /// Crée une nouvelle couleur.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<ColorDto>> CreateColor([FromBody] ColorCreateDto createDto)
    {
        var validationResult = await colorCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Vérifier si le nom existe déjà (basé sur notre contrainte unique)
        if (await Context.Colors.AnyAsync(c => c.Name == createDto.Name))
        {
            // Retourne un 409 Conflict si le nom est déjà pris
            return Conflict("Une couleur avec ce nom existe déjà.");
        }

        // Mapper le DTO vers l'Entité
        var newColor = new Color
        {
            Name = createDto.Name,
            HexValue = createDto.HexValue
        };
        
        // Note : L'Id, CreatedAt, UpdatedAt sont gérés par le DbContext (SaveChanges)

        Context.Colors.Add(newColor);
        await Context.SaveChangesAsync();

        // Mapper l'entité créée vers le DTO de retour
        var colorDto = new ColorDto
        {
            Id = newColor.Id,
            Name = newColor.Name,
            HexValue = newColor.HexValue
        };

        // Retourne un 201 Created avec l'URL pour la nouvelle ressource
        return CreatedAtAction(nameof(GetColorById), new { id = colorDto.Id }, colorDto);
    }

    /// <summary>
    /// PUT: api/colors/{id}
    /// Met à jour une couleur existante.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdateColor(Guid id, [FromBody] ColorUpdateDto updateDto)
    {
        var validationResult = await colorUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        var colorToUpdate = await Context.Colors.FindAsync(id);

        if (colorToUpdate == null)
        {
            return NotFound("La couleur à mettre à jour n'a pas été trouvée.");
        }

        // Vérifier si le nouveau nom est déjà pris par une *autre* couleur
        if (await Context.Colors.AnyAsync(c => c.Name == updateDto.Name && c.Id != id))
        {
            return Conflict("Une autre couleur avec ce nom existe déjà.");
        }

        // Appliquer les modifications du DTO à l'entité
        colorToUpdate.Name = updateDto.Name;
        colorToUpdate.HexValue = updateDto.HexValue;
        // UpdatedAt sera géré par le DbContext (SaveChanges)

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Gérer le cas où l'entité a été supprimée par un autre utilisateur
            // entre le FindAsync et le SaveChangesAsync
            if (!Context.Colors.Any(c => c.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        // Retourne un 204 No Content, signifiant que la MàJ a réussi
        return NoContent();
    }

    /// <summary>
    /// DELETE: api/colors/{id}
    /// Supprime une couleur.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteColor(Guid id)
    {
        var colorToDelete = await Context.Colors.FindAsync(id);

        if (colorToDelete == null)
        {
            return NotFound("La couleur à supprimer n'a pas été trouvée.");
        }

        // Vérification de sécurité : une couleur est-elle utilisée par un patient ?
        var isUsed = await Context.Entry(colorToDelete)
            .Collection(c => c.Patients)
            .Query() // Permet de faire une requête sur la collection
            .AnyAsync();

        if (isUsed)
        {
            // On ne supprime pas une couleur si elle est assignée à un animal
            return BadRequest("Cette couleur ne peut pas être supprimée car elle est utilisée par un ou plusieurs patients.");
        }

        Context.Colors.Remove(colorToDelete);
        await Context.SaveChangesAsync();

        // Retourne un 204 No Content, signifiant que la suppression a réussi
        return NoContent();
    }
}