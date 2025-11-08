using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using VetoPro.Api.Data;
using VetoPro.Api.Helpers;

namespace VetoPro.Api.Controllers;

/// <summary>
/// Un contrôleur de base pour l'application VetoPro.
/// Il fournit un accès partagé au DbContext et des méthodes utilitaires
/// pour l'authentification, comme la récupération du ContactId de l'utilisateur.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(VetoProDbContext context) : ControllerBase
{
    // 'protected' le rend accessible à toutes les classes qui en héritent
    protected readonly VetoProDbContext Context = context;

    /// <summary>
    /// Récupère l'ID du Contact lié à l'utilisateur JWT actuel.
    /// Cette méthode est maintenant centralisée et réutilisable.
    /// </summary>
    protected async Task<(Guid? ContactId, ActionResult? ErrorResult)> GetUserContactId()
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Nous n'avons pas besoin de vérifier string.IsNullOrEmpty
        // car l'[Authorize] en haut des contrôleurs garantit que l'utilisateur est connecté.
        // Mais par sécurité, nous le gardons.
        if (string.IsNullOrEmpty(currentUserIdString))
        {
            return (null, Unauthorized("Token non valide."));
        }

        var userContact = await Context.Contacts
            .Where(c => c.UserId.ToString() == currentUserIdString.ToUpper())
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (userContact == Guid.Empty)
        {
            // Cela signifie que l'utilisateur (IdentityUser) existe, 
            // mais qu'aucun profil Contact n'est lié.
            return (null, Forbid("Aucun profil de contact n'est lié à ce compte utilisateur."));
        }

        return (userContact, null);
    }
    
    /// <summary>
    /// Creates a paginated ActionResult from an IQueryable source.
    /// Handles calling PagedList.CreateAsync, adding pagination headers,
    /// and returning the appropriate Ok result.
    /// </summary>
    /// <remarks>
    /// This helper assumes we map before pagination. This is usually fine for simple mappings.
    /// If a mapping is very complex and can't be translated to SQL, we might need a variation
    /// of this helper that maps after ToListAsync.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity coming from the database.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to return.</typeparam>
    /// <param name="query">The IQueryable source query (before mapping or pagination).</param>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <param name="mapper">A function that maps an entity TEntity to a DTO TDto.</param>
    /// <returns>An ActionResult containing the list of TDto items for the current page.</returns>
    protected async Task<ActionResult<IEnumerable<TDto>>> CreatePaginatedResponse<TEntity, TDto>(
        IQueryable<TEntity> query,
        PaginationParams paginationParams,
        Func<TEntity, TDto> mapper)
        where TEntity : class // Constrain TEntity to be a class (like your entities)
        where TDto : class    // Constrain TDto to be a class (like your DTOs)
    {
        // Apply mapping *before* pagination if the mapper is simple
        // If the mapper is complex or causes issues with translation, apply it after ToListAsync
        var dtoQuery = query.Select(entity => mapper(entity));

        var pagedList = await PagedList<TDto>.CreateAsync(
            dtoQuery,
            paginationParams.PageNumber,
            paginationParams.PageSize);

        // Add pagination headers
        var paginationMetadata = new
        {
            pagedList.TotalCount,
            pagedList.PageSize,
            pagedList.CurrentPage,
            pagedList.TotalPages,
            pagedList.HasNext,
            pagedList.HasPrevious
        };
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        // Return only the items
        return Ok(pagedList.Items);
    }
}