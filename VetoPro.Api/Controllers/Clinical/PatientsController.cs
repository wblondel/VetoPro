using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Entities;
using VetoPro.Api.Helpers;
using VetoPro.Api.Mapping;
using VetoPro.Contracts.DTOs;
using VetoPro.Contracts.DTOs.Clinical;

namespace VetoPro.Api.Controllers.Clinical;

[Authorize]
public class PatientsController(
    VetoProDbContext context,
    IValidator<PatientCreateDto> patientCreateValidator,
    IValidator<PatientUpdateDto> patientUpdateValidator)
    : BaseApiController(context)
{
    /// <summary>
    /// GET: api/patients
    /// Récupère la liste de tous les patients avec leurs détails.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients([FromQuery] PaginationParams paginationParams)
    {
        var query = Context.Patients
            .Include(p => p.Owner) // Jointure avec Contacts (Propriétaire)
            .Include(p => p.Breed)
                .ThenInclude(b => b.Species) // Jointure imbriquée avec Species
            .Include(p => p.Colors) // Jointure avec Colors (M2M)
            .OrderBy(p => p.Name)
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, p => p.ToDto());
    }

    /// <summary>
    /// GET: api/patients/{id}
    /// Récupère un patient spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> GetPatientById(Guid id)
    {
        var patient = await Context.Patients
            .Include(p => p.Owner)
            .Include(p => p.Breed)
                .ThenInclude(b => b.Species)
            .Include(p => p.Colors)
            .Where(p => p.Id == id)
            .Select(p => p.ToDto())
            .FirstOrDefaultAsync();

        if (patient == null)
        {
            return NotFound("Patient non trouvé.");
        }

        if (User.IsInRole("Admin") || User.IsInRole("Doctor"))
        {
            return Ok(patient);
        }
        
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;

        if (patient.OwnerId != userContactId)
        {
            return Forbid();
        }
        
        return Ok(patient);
    }

    /// <summary>
    /// GET: api/patients/{id}/consultations
    /// Récupère l'historique de toutes les consultations pour un patient donné.
    /// </summary>
    [HttpGet("{id}/consultations")]
    public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetConsultationsForPatient(Guid id)
    {
        var patient = await Context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound("Patient non trouvé.");
        }
        
        if (!User.IsInRole("Admin") && !User.IsInRole("Doctor"))
        {
            var (userContactId, errorResult) = await GetUserContactId();
            if (errorResult != null) return errorResult;
            
            if (patient.OwnerId != userContactId)
            {
                return Forbid();
            }
        }

        var consultations = await Context.Consultations
            .Include(c => c.Client)
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .Where(c => c.PatientId == id) // Filtrer par l'ID du patient
            .OrderByDescending(c => c.ConsultationDate) // Du plus récent au plus ancien
            .Select(c => c.ToDto()) // Utiliser le mapper de consultation
            .ToListAsync();

        return Ok(consultations);
    }
    
    /// <summary>
    /// POST: api/patients
    /// Crée un nouveau patient.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] PatientCreateDto createDto)
    {
        var validationResult = await patientCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // 1. Valider les clés étrangères (et récupérer les entités)
        var owner = await Context.Contacts.FindAsync(createDto.OwnerId);
        if (owner == null)
        {
            return BadRequest("L'ID du propriétaire (OwnerId) n'existe pas.");
        }
        
        var breed = await Context.Breeds.Include(b => b.Species).FirstOrDefaultAsync(b => b.Id == createDto.BreedId);
        if (breed == null)
        {
            return BadRequest("L'ID de la race (BreedId) n'existe pas.");
        }

        // 2. Valider et récupérer les entités Couleurs (M2M)
        var colors = await Context.Colors
            .Where(c => createDto.ColorIds.Contains(c.Id))
            .ToListAsync();
        
        if (colors.Count != createDto.ColorIds.Count)
        {
            return BadRequest("Une ou plusieurs IDs de couleur (ColorIds) n'existent pas.");
        }

        // 3. Mapper et créer l'entité Patient
        var newPatient = new Patient
        {
            Name = createDto.Name,
            OwnerId = createDto.OwnerId,
            BreedId = createDto.BreedId,
            ChipNumber = createDto.ChipNumber,
            DobEstimateStart = createDto.DobEstimateStart,
            DobEstimateEnd = createDto.DobEstimateEnd,
            Gender = createDto.Gender,
            ReproductiveStatus = createDto.ReproductiveStatus,
            Colors = colors // Assigner la liste d'entités Couleur
        };

        Context.Patients.Add(newPatient);
        await Context.SaveChangesAsync();

        // 4. Mapper au DTO de retour
        newPatient.Owner = owner; // 'owner' est maintenant un 'Contact' non-null
        newPatient.Breed = breed; // 'breed' inclut déjà 'Species'
        
        var patientDto = newPatient.ToDto();

        return CreatedAtAction(nameof(GetPatientById), new { id = patientDto.Id }, patientDto);
    }

    /// <summary>
    /// PUT: api/patients/{id}
    /// Met à jour un patient existant.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] PatientUpdateDto updateDto)
    {
        var validationResult = await patientUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // 1. Trouver le patient et ses couleurs actuelles
        var patientToUpdate = await Context.Patients
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patientToUpdate == null)
        {
            return NotFound("Patient non trouvé.");
        }

        // 2. Valider les clés étrangères
        if (updateDto.OwnerId != patientToUpdate.OwnerId && !await Context.Contacts.AnyAsync(c => c.Id == updateDto.OwnerId))
        {
            return BadRequest("Le nouvel ID du propriétaire (OwnerId) n'existe pas.");
        }
        if (updateDto.BreedId != patientToUpdate.BreedId && !await Context.Breeds.AnyAsync(b => b.Id == updateDto.BreedId))
        {
            return BadRequest("Le nouvel ID de la race (BreedId) n'existe pas.");
        }

        // 3. Mettre à jour les propriétés simples
        patientToUpdate.Name = updateDto.Name;
        patientToUpdate.OwnerId = updateDto.OwnerId;
        patientToUpdate.BreedId = updateDto.BreedId;
        patientToUpdate.ChipNumber = updateDto.ChipNumber;
        patientToUpdate.DobEstimateStart = updateDto.DobEstimateStart;
        patientToUpdate.DobEstimateEnd = updateDto.DobEstimateEnd;
        patientToUpdate.Gender = updateDto.Gender;
        patientToUpdate.ReproductiveStatus = updateDto.ReproductiveStatus;
        patientToUpdate.DeceasedAt = updateDto.DeceasedAt;

        // 4. Gérer la mise à jour de la relation M2M (Couleurs)
        // Supprimer les anciennes, ajouter les nouvelles
        patientToUpdate.Colors.Clear();
        var newColors = await Context.Colors
            .Where(c => updateDto.ColorIds.Contains(c.Id))
            .ToListAsync();
        
        if (newColors.Count != updateDto.ColorIds.Count)
        {
            return BadRequest("Une ou plusieurs IDs de couleur (ColorIds) n'existent pas.");
        }

        foreach (var color in newColors)
        {
            patientToUpdate.Colors.Add(color);
        }

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!Context.Patients.Any(p => p.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/patients/{id}
    /// Supprime un patient.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var patientToDelete = await Context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.Consultations)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patientToDelete == null)
        {
            return NotFound("Patient non trouvé.");
        }

        // Sécurité : Vérifier les dépendances
        if (patientToDelete.Appointments.Any() || patientToDelete.Consultations.Any())
        {
            return BadRequest("Ce patient ne peut pas être supprimé car il est lié à des rendez-vous ou des consultations.");
        }

        // Note : La suppression de la table de liaison M2M (patient_colors)
        // est gérée automatiquement par EF Core.

        Context.Patients.Remove(patientToDelete);
        await Context.SaveChangesAsync();

        return NoContent();
    }
}