using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/contacts
public class ContactsController : ControllerBase
{
    private readonly VetoProDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public ContactsController(
        VetoProDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// GET: api/contacts
    /// Récupère une liste de tous les contacts.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactDto>>> GetAllContacts()
    {
        var contacts = await _context.Contacts
            .Include(c => c.User) // Inclure le compte de connexion
            .Include(c => c.StaffDetails) // Inclure les détails du staff
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => MapToContactDto(c)) // Utiliser une méthode de mapping
            .ToListAsync();
            
        return Ok(contacts);
    }

    /// <summary>
    /// GET: api/contacts/{id}
    /// Récupère un contact spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDto>> GetContactById(Guid id)
    {
        var contact = await _context.Contacts
            .Include(c => c.User)
            .Include(c => c.StaffDetails)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
        {
            return NotFound("Contact non trouvé.");
        }

        return Ok(MapToContactDto(contact));
    }

    /// <summary>
    /// POST: api/contacts
    /// Crée un nouveau contact, avec optionnellement un compte de connexion et des détails de staff.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContactDto>> CreateContact([FromBody] ContactCreateDto createDto)
    {
        // Validation logique
        if (createDto.IsStaff && createDto.StaffDetails == null)
        {
            return BadRequest("Les détails du staff (StaffDetails) sont obligatoires si 'IsStaff' est vrai.");
        }
        
        // Commencer une transaction : si une partie échoue, tout est annulé
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Gérer le Compte de Connexion (si fourni)
            ApplicationUser? newUser = null;
            if (createDto.Account != null)
            {
                // Vérifier si l'e-mail de connexion est déjà pris
                if (await _userManager.FindByEmailAsync(createDto.Account.LoginEmail) != null)
                {
                    return Conflict("Cet e-mail de connexion est déjà utilisé.");
                }

                newUser = new ApplicationUser
                {
                    UserName = createDto.Account.LoginEmail,
                    Email = createDto.Account.LoginEmail,
                    EmailConfirmed = true // Confirmer auto pour le dev
                };

                var identityResult = await _userManager.CreateAsync(newUser, createDto.Account.Password);

                if (!identityResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(identityResult.Errors);
                }
                
                // Assigner les rôles
                if (createDto.IsStaff)
                {
                    // Assigner le rôle "Doctor" ou "Admin" basé sur createDto.StaffDetails.Role
                    await _userManager.AddToRoleAsync(newUser, createDto.StaffDetails!.Role); 
                }
                else
                {
                    await _userManager.AddToRoleAsync(newUser, "Client");
                }
            }

            // 2. Créer l'entité Contact
            var newContact = new Contact
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.ContactEmail,
                PhoneNumber = createDto.PhoneNumber,
                AddressLine1 = createDto.AddressLine1,
                City = createDto.City,
                PostalCode = createDto.PostalCode,
                Country = createDto.Country,
                IsOwner = createDto.IsOwner,
                IsClient = createDto.IsClient,
                IsStaff = createDto.IsStaff,
                UserId = newUser?.Id // Lier le compte de connexion s'il a été créé
            };
            
            _context.Contacts.Add(newContact);
            // Il faut sauvegarder ici pour que newContact.Id soit généré
            await _context.SaveChangesAsync();

            // 3. Gérer les Détails du Staff (si 'IsStaff' est vrai)
            if (createDto.IsStaff && createDto.StaffDetails != null)
            {
                var newStaffDetails = new StaffDetails
                {
                    ContactId = newContact.Id, // Lier au contact qu'on vient de créer
                    Role = createDto.StaffDetails.Role,
                    LicenseNumber = createDto.StaffDetails.LicenseNumber,
                    Specialty = createDto.StaffDetails.Specialty,
                    IsActive = createDto.StaffDetails.IsActive
                };
                _context.StaffDetails.Add(newStaffDetails);
                
                // Sauvegarder les détails du staff
                await _context.SaveChangesAsync();
                
                // Remplir la propriété de navigation pour le DTO de retour
                newContact.StaffDetails = newStaffDetails; 
            }

            // 4. Valider la transaction
            await transaction.CommitAsync();

            // 5. Remplir le DTO de retour et renvoyer 201 Created
            newContact.User = newUser; // Attacher l'utilisateur pour le mapping
            var contactDto = MapToContactDto(newContact);

            return CreatedAtAction(nameof(GetContactById), new { id = contactDto.Id }, contactDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
        }
    }

    /// <summary>
    /// PUT: api/contacts/{id}
    /// Met à jour un contact existant.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] ContactUpdateDto updateDto)
    {
        var contactToUpdate = await _context.Contacts
            .Include(c => c.StaffDetails) // Charger les détails staff existants
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contactToUpdate == null)
        {
            return NotFound("Contact non trouvé.");
        }

        // Appliquer les modifications du profil de base
        contactToUpdate.FirstName = updateDto.FirstName;
        contactToUpdate.LastName = updateDto.LastName;
        contactToUpdate.Email = updateDto.ContactEmail;
        contactToUpdate.PhoneNumber = updateDto.PhoneNumber;
        contactToUpdate.AddressLine1 = updateDto.AddressLine1;
        contactToUpdate.City = updateDto.City;
        contactToUpdate.PostalCode = updateDto.PostalCode;
        contactToUpdate.Country = updateDto.Country;
        contactToUpdate.IsOwner = updateDto.IsOwner;
        contactToUpdate.IsClient = updateDto.IsClient;
        contactToUpdate.IsStaff = updateDto.IsStaff;

        // Gérer la logique des détails du staff
        if (updateDto.IsStaff)
        {
            if (updateDto.StaffDetails == null)
            {
                return BadRequest("Les détails du staff (StaffDetails) sont obligatoires si 'IsStaff' est vrai.");
            }

            if (contactToUpdate.StaffDetails == null)
            {
                // Cas 1: Devient Staff -> Créer les détails
                var newStaffDetails = new StaffDetails
                {
                    ContactId = contactToUpdate.Id,
                    Role = updateDto.StaffDetails.Role,
                    LicenseNumber = updateDto.StaffDetails.LicenseNumber,
                    Specialty = updateDto.StaffDetails.Specialty,
                    IsActive = updateDto.StaffDetails.IsActive
                };
                _context.StaffDetails.Add(newStaffDetails);
            }
            else
            {
                // Cas 2: Était déjà Staff -> Mettre à jour les détails
                contactToUpdate.StaffDetails.Role = updateDto.StaffDetails.Role;
                contactToUpdate.StaffDetails.LicenseNumber = updateDto.StaffDetails.LicenseNumber;
                contactToUpdate.StaffDetails.Specialty = updateDto.StaffDetails.Specialty;
                contactToUpdate.StaffDetails.IsActive = updateDto.StaffDetails.IsActive;
            }
        }
        else if (contactToUpdate.StaffDetails != null)
        {
            // Cas 3: N'est plus Staff -> Supprimer les détails
            _context.StaffDetails.Remove(contactToUpdate.StaffDetails);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Contacts.Any(c => c.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/contacts/{id}
    /// Supprime un contact (profil et compte de connexion).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(Guid id)
    {
        var contactToDelete = await _context.Contacts
            .Include(c => c.PatientsOwned)
            .Include(c => c.Invoices)
            .Include(c => c.StaffDetails)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contactToDelete == null)
        {
            return NotFound("Contact non trouvé.");
        }

        // Sécurité : Vérifier les dépendances
        if (contactToDelete.PatientsOwned.Any())
        {
            return BadRequest("Ce contact ne peut pas être supprimé car il est propriétaire d'un ou plusieurs patients.");
        }
        if (contactToDelete.Invoices.Any())
        {
            return BadRequest("Ce contact ne peut pas être supprimé car il est lié à une ou plusieurs factures.");
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Supprimer les détails Staff (si existent)
            if (contactToDelete.StaffDetails != null)
            {
                _context.StaffDetails.Remove(contactToDelete.StaffDetails);
            }

            // 2. Supprimer le contact
            _context.Contacts.Remove(contactToDelete);
            
            // Sauvegarder la suppression du contact et staff
            await _context.SaveChangesAsync(); 

            // 3. Supprimer le compte de connexion (ApplicationUser)
            if (contactToDelete.UserId != null)
            {
                var userToDelete = await _userManager.FindByIdAsync(contactToDelete.UserId.Value.ToString());
                if (userToDelete != null)
                {
                    var identityResult = await _userManager.DeleteAsync(userToDelete);
                    if (!identityResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(identityResult.Errors);
                    }
                }
            }

            await transaction.CommitAsync();
            
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
        }
    }


    /// <summary>
    /// Méthode privée pour mapper une entité Contact vers un ContactDto.
    /// </summary>
    private static ContactDto MapToContactDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            LoginEmail = contact.User?.Email, // Email du compte Identity
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            ContactEmail = contact.Email, // Email du profil
            PhoneNumber = contact.PhoneNumber,
            AddressLine1 = contact.AddressLine1,
            City = contact.City,
            PostalCode = contact.PostalCode,
            Country = contact.Country,
            IsOwner = contact.IsOwner,
            IsClient = contact.IsClient,
            IsStaff = contact.IsStaff,
            StaffDetails = contact.StaffDetails == null ? null : new StaffDetailsDto
            {
                Id = contact.StaffDetails.Id,
                Role = contact.StaffDetails.Role,
                LicenseNumber = contact.StaffDetails.LicenseNumber,
                Specialty = contact.StaffDetails.Specialty,
                IsActive = contact.StaffDetails.IsActive
            }
        };
    }
}