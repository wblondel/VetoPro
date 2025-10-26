using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;
using VetoPro.Api.Entities; // Nécessaire pour l'entité 'Color'

namespace VetoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/colors
public class ColorsController : ControllerBase
{
    private readonly VetoProDbContext _context;

    public ColorsController(VetoProDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: api/colors
    /// Récupère la liste de toutes les couleurs.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ColorDto>>> GetAllColors()
    {
        var colors = await _context.Colors
            .OrderBy(c => c.Name)
            .Select(c => new ColorDto
            {
                Id = c.Id,
                Name = c.Name,
                HexValue = c.HexValue
            })
            .ToListAsync();

        return Ok(colors);
    }

    /// <summary>
    /// GET: api/colors/{id}
    /// Récupère une couleur spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ColorDto>> GetColorById(Guid id)
    {
        var color = await _context.Colors
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
    public async Task<ActionResult<ColorDto>> CreateColor([FromBody] ColorCreateDto createDto)
    {
        // Vérifier si le nom existe déjà (basé sur notre contrainte unique)
        if (await _context.Colors.AnyAsync(c => c.Name == createDto.Name))
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

        _context.Colors.Add(newColor);
        await _context.SaveChangesAsync();

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
    public async Task<IActionResult> UpdateColor(Guid id, [FromBody] ColorUpdateDto updateDto)
    {
        var colorToUpdate = await _context.Colors.FindAsync(id);

        if (colorToUpdate == null)
        {
            return NotFound("La couleur à mettre à jour n'a pas été trouvée.");
        }

        // Vérifier si le nouveau nom est déjà pris par une *autre* couleur
        if (await _context.Colors.AnyAsync(c => c.Name == updateDto.Name && c.Id != id))
        {
            return Conflict("Une autre couleur avec ce nom existe déjà.");
        }

        // Appliquer les modifications du DTO à l'entité
        colorToUpdate.Name = updateDto.Name;
        colorToUpdate.HexValue = updateDto.HexValue;
        // UpdatedAt sera géré par le DbContext (SaveChanges)

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Gérer le cas où l'entité a été supprimée par un autre utilisateur
            // entre le FindAsync et le SaveChangesAsync
            if (!_context.Colors.Any(c => c.Id == id))
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
    public async Task<IActionResult> DeleteColor(Guid id)
    {
        var colorToDelete = await _context.Colors.FindAsync(id);

        if (colorToDelete == null)
        {
            return NotFound("La couleur à supprimer n'a pas été trouvée.");
        }

        // Vérification de sécurité : une couleur est-elle utilisée par un patient ?
        var isUsed = await _context.Entry(colorToDelete)
            .Collection(c => c.Patients)
            .Query() // Permet de faire une requête sur la collection
            .AnyAsync();

        if (isUsed)
        {
            // On ne supprime pas une couleur si elle est assignée à un animal
            return BadRequest("Cette couleur ne peut pas être supprimée car elle est utilisée par un ou plusieurs patients.");
        }

        _context.Colors.Remove(colorToDelete);
        await _context.SaveChangesAsync();

        // Retourne un 204 No Content, signifiant que la suppression a réussi
        return NoContent();
    }
}