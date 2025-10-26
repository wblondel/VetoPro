using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Controllers;

[Authorize]
public class ServicesController(VetoProDbContext context) : BaseApiController(context)
{
    /// <summary>
    /// GET: api/services
    /// Récupère la liste de tous les services (actes).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetAllServices()
    {
        var services = await _context.Services
            .OrderBy(s => s.Name)
            .Select(s => MapToServiceDto(s))
            .ToListAsync();

        return Ok(services);
    }

    /// <summary>
    /// GET: api/services/{id}
    /// Récupère un service spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceDto>> GetServiceById(Guid id)
    {
        var service = await _context.Services
            .Where(s => s.Id == id)
            .Select(s => MapToServiceDto(s))
            .FirstOrDefaultAsync();

        if (service == null)
        {
            return NotFound("Service non trouvé.");
        }

        return Ok(service);
    }

    /// <summary>
    /// POST: api/services
    /// Crée un nouveau service (acte).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<ServiceDto>> CreateService([FromBody] ServiceCreateDto createDto)
    {
        // Vérifier si le nom existe déjà (contrainte unique)
        if (await _context.Services.AnyAsync(s => s.Name == createDto.Name))
        {
            return Conflict("Un service avec ce nom existe déjà.");
        }

        // Mapper le DTO vers l'Entité
        var newService = new Service
        {
            Name = createDto.Name,
            Description = createDto.Description,
            IsActive = createDto.IsActive,
            RequiresPatient = createDto.RequiresPatient
        };

        _context.Services.Add(newService);
        await _context.SaveChangesAsync();

        // Mapper l'entité créée vers le DTO de retour
        var serviceDto = MapToServiceDto(newService);

        return CreatedAtAction(nameof(GetServiceById), new { id = serviceDto.Id }, serviceDto);
    }

    /// <summary>
    /// PUT: api/services/{id}
    /// Met à jour un service existant.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] ServiceUpdateDto updateDto)
    {
        var serviceToUpdate = await _context.Services.FindAsync(id);

        if (serviceToUpdate == null)
        {
            return NotFound("Service non trouvé.");
        }

        // Vérifier si le nouveau nom est déjà pris par un *autre* service
        if (await _context.Services.AnyAsync(s => s.Name == updateDto.Name && s.Id != id))
        {
            return Conflict("Un autre service avec ce nom existe déjà.");
        }

        // Appliquer les modifications
        serviceToUpdate.Name = updateDto.Name;
        serviceToUpdate.Description = updateDto.Description;
        serviceToUpdate.IsActive = updateDto.IsActive;
        serviceToUpdate.RequiresPatient = updateDto.RequiresPatient;

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/services/{id}
    /// Supprime un service.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var serviceToDelete = await _context.Services.FindAsync(id);

        if (serviceToDelete == null)
        {
            return NotFound("Service non trouvé.");
        }

        // Sécurité : Vérifier si le service est utilisé dans une PriceList
        var isUsedInPriceList = await _context.PriceList.AnyAsync(pl => pl.ServiceId == id);
        if (isUsedInPriceList)
        {
            return BadRequest("Ce service ne peut pas être supprimé car il est utilisé dans la liste de prix (PriceList).");
        }

        // Sécurité : Vérifier si le service est utilisé dans une InvoiceLine
        var isUsedInInvoice = await _context.InvoiceLines.AnyAsync(il => il.ServiceId == id);
        if (isUsedInInvoice)
        {
            return BadRequest("Ce service ne peut pas être supprimé car il est lié à une ou plusieurs factures.");
        }

        _context.Services.Remove(serviceToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    /// <summary>
    /// Méthode privée pour mapper une entité Service vers un ServiceDto.
    /// </summary>
    private static ServiceDto MapToServiceDto(Service s)
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