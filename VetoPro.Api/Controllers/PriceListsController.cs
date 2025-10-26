using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using VetoPro.Api.Mapping;
using VetoPro.Api.Helpers;

namespace VetoPro.Api.Controllers;

[Authorize]
public class PriceListsController(VetoProDbContext context) : BaseApiController(context)
{
    /// <summary>
    /// GET: api/pricelists
    /// Récupère toutes les règles de prix, avec les noms des services et espèces.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PriceListDto>>> GetAllPriceRules([FromQuery] PaginationParams paginationParams)
    {
        var query = _context.PriceList
            .Include(pl => pl.Service) // Jointure avec Services
            .Include(pl => pl.Species) // Jointure avec Species (gère les null)
            .OrderBy(pl => pl.Service.Name).ThenBy(pl => pl.Species.Name)
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, pl => pl.ToDto());
    }

    /// <summary>
    /// GET: api/pricelists/{id}
    /// Récupère une règle de prix spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PriceListDto>> GetPriceRuleById(Guid id)
    {
        var priceRule = await _context.PriceList
            .Include(pl => pl.Service)
            .Include(pl => pl.Species)
            .Where(pl => pl.Id == id)
            .Select(pl => pl.ToDto())
            .FirstOrDefaultAsync();

        if (priceRule == null)
        {
            return NotFound("Règle de prix non trouvée.");
        }

        return Ok(priceRule);
    }
    
    /// <summary>
    /// GET: api/pricelists/for-service/{serviceId}
    /// Récupère toutes les règles de prix pour un service donné.
    /// </summary>
    [HttpGet("for-service/{serviceId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PriceListDto>>> GetPriceRulesForService(Guid serviceId)
    {
        if (!await _context.Services.AnyAsync(s => s.Id == serviceId))
        {
            return NotFound("Service non trouvé.");
        }
        
        var priceRules = await _context.PriceList
            .Include(pl => pl.Service)
            .Include(pl => pl.Species)
            .Where(pl => pl.ServiceId == serviceId)
            .OrderBy(pl => pl.Species.Name).ThenBy(pl => pl.WeightMinKg)
            .Select(pl => pl.ToDto())
            .ToListAsync();

        return Ok(priceRules);
    }

    /// <summary>
    /// POST: api/pricelists
    /// Crée une nouvelle règle de prix.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<PriceListDto>> CreatePriceRule([FromBody] PriceListCreateDto createDto)
    {
        // 1. Valider les clés étrangères
        if (!await _context.Services.AnyAsync(s => s.Id == createDto.ServiceId))
        {
            return BadRequest("L'ID du service (ServiceId) n'existe pas.");
        }
        if (createDto.SpeciesId.HasValue && !await _context.Species.AnyAsync(s => s.Id == createDto.SpeciesId.Value))
        {
            return BadRequest("L'ID de l'espèce (SpeciesId) n'existe pas.");
        }
        
        // 2. Valider la logique (ex: poids min < poids max)
        if (createDto.WeightMinKg.HasValue && createDto.WeightMaxKg.HasValue && createDto.WeightMinKg > createDto.WeightMaxKg)
        {
            return BadRequest("Le poids minimum ne peut pas être supérieur au poids maximum.");
        }
        
        // 3. Mapper le DTO vers l'Entité
        var newPriceRule = new PriceList
        {
            ServiceId = createDto.ServiceId,
            SpeciesId = createDto.SpeciesId,
            WeightMinKg = createDto.WeightMinKg,
            WeightMaxKg = createDto.WeightMaxKg,
            Amount = createDto.Amount,
            Currency = createDto.Currency,
            IsActive = createDto.IsActive
        };

        _context.PriceList.Add(newPriceRule);
        await _context.SaveChangesAsync();

        // 4. Mapper en retour pour la réponse 201
        var createdPriceRule = await _context.PriceList
            .Include(pl => pl.Service)
            .Include(pl => pl.Species)
            .FirstAsync(pl => pl.Id == newPriceRule.Id);

        return CreatedAtAction(nameof(GetPriceRuleById), new { id = createdPriceRule.Id }, createdPriceRule.ToDto());
    }

    /// <summary>
    /// PUT: api/pricelists/{id}
    /// Met à jour une règle de prix existante.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdatePriceRule(Guid id, [FromBody] PriceListUpdateDto updateDto)
    {
        var ruleToUpdate = await _context.PriceList.FindAsync(id);

        if (ruleToUpdate == null)
        {
            return NotFound("Règle de prix non trouvée.");
        }

        // 1. Valider les clés étrangères (si elles changent)
        if (updateDto.ServiceId != ruleToUpdate.ServiceId && !await _context.Services.AnyAsync(s => s.Id == updateDto.ServiceId))
        {
            return BadRequest("Le nouvel ID service n'existe pas.");
        }
        if (updateDto.SpeciesId != ruleToUpdate.SpeciesId && 
            updateDto.SpeciesId.HasValue && 
            !await _context.Species.AnyAsync(s => s.Id == updateDto.SpeciesId.Value))
        {
            return BadRequest("Le nouvel ID espèce n'existe pas.");
        }
        
        // 2. Valider la logique (ex: poids min < poids max)
        if (updateDto.WeightMinKg.HasValue && updateDto.WeightMaxKg.HasValue && updateDto.WeightMinKg > updateDto.WeightMaxKg)
        {
            return BadRequest("Le poids minimum ne peut pas être supérieur au poids maximum.");
        }

        // 3. Appliquer les modifications
        ruleToUpdate.ServiceId = updateDto.ServiceId;
        ruleToUpdate.SpeciesId = updateDto.SpeciesId;
        ruleToUpdate.WeightMinKg = updateDto.WeightMinKg;
        ruleToUpdate.WeightMaxKg = updateDto.WeightMaxKg;
        ruleToUpdate.Amount = updateDto.Amount;
        ruleToUpdate.Currency = updateDto.Currency;
        ruleToUpdate.IsActive = updateDto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/pricelists/{id}
    /// Supprime une règle de prix.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePriceRule(Guid id)
    {
        var ruleToDelete = await _context.PriceList.FindAsync(id);

        if (ruleToDelete == null)
        {
            return NotFound("Règle de prix non trouvée.");
        }
        
        // Il n'y a pas de dépendance bloquante directe sur cette table,
        // la suppression est donc "sécurisée" (elle ne casse pas de factures existantes).
        _context.PriceList.Remove(ruleToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}