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
public class ServicesController(
    VetoProDbContext context,
    IValidator<ServiceCreateDto> serviceCreateValidator,
    IValidator<ServiceUpdateDto> serviceUpdateValidator)
    : BaseApiController(context)
{
    /// <summary>
    /// GET: api/services
    /// Récupère la liste de tous les services (actes).
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetAllServices([FromQuery] PaginationParams paginationParams)
    {
        var query = Context.Services
            .OrderBy(s => s.Name)
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, s => s.ToDto());
    }

    /// <summary>
    /// GET: api/services/{id}
    /// Récupère un service spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceDto>> GetServiceById(Guid id)
    {
        var service = await Context.Services
            .Where(s => s.Id == id)
            .Select(s => s.ToDto())
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
        var validationResult = await serviceCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Vérifier si le nom existe déjà (contrainte unique)
        if (await Context.Services.AnyAsync(s => s.Name == createDto.Name))
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

        Context.Services.Add(newService);
        await Context.SaveChangesAsync();

        // Mapper l'entité créée vers le DTO de retour
        var serviceDto = newService.ToDto();

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
        var validationResult = await serviceUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        var serviceToUpdate = await Context.Services.FindAsync(id);

        if (serviceToUpdate == null)
        {
            return NotFound("Service non trouvé.");
        }

        // Vérifier si le nouveau nom est déjà pris par un *autre* service
        if (await Context.Services.AnyAsync(s => s.Name == updateDto.Name && s.Id != id))
        {
            return Conflict("Un autre service avec ce nom existe déjà.");
        }

        // Appliquer les modifications
        serviceToUpdate.Name = updateDto.Name;
        serviceToUpdate.Description = updateDto.Description;
        serviceToUpdate.IsActive = updateDto.IsActive;
        serviceToUpdate.RequiresPatient = updateDto.RequiresPatient;

        await Context.SaveChangesAsync();

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
        var serviceToDelete = await Context.Services.FindAsync(id);

        if (serviceToDelete == null)
        {
            return NotFound("Service non trouvé.");
        }

        // Sécurité : Vérifier si le service est utilisé dans une PriceList
        var isUsedInPriceList = await Context.PriceList.AnyAsync(pl => pl.ServiceId == id);
        if (isUsedInPriceList)
        {
            return BadRequest("Ce service ne peut pas être supprimé car il est utilisé dans la liste de prix (PriceList).");
        }

        // Sécurité : Vérifier si le service est utilisé dans une InvoiceLine
        var isUsedInInvoice = await Context.InvoiceLines.AnyAsync(il => il.ServiceId == id);
        if (isUsedInInvoice)
        {
            return BadRequest("Ce service ne peut pas être supprimé car il est lié à une ou plusieurs factures.");
        }

        Context.Services.Remove(serviceToDelete);
        await Context.SaveChangesAsync();

        return NoContent();
    }
}