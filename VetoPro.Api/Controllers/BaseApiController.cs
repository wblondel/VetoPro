using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VetoPro.Api.Data;

namespace VetoPro.Api.Controllers;

/// <summary>
/// Un contrôleur de base pour l'application VetoPro.
/// Il fournit un accès partagé au DbContext et des méthodes utilitaires
/// pour l'authentification, comme la récupération du ContactId de l'utilisateur.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    // 'protected' le rend accessible à toutes les classes qui en héritent
    protected readonly VetoProDbContext _context;

    // Le constructeur de base reçoit le contexte
    protected BaseApiController(VetoProDbContext context)
    {
        _context = context;
    }

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

        var userContact = await _context.Contacts
            .Where(c => c.UserId.ToString() == currentUserIdString)
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
}