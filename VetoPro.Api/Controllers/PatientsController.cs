using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/patients
public class PatientsController : ControllerBase
{
    private readonly VetoProDbContext _context;

    public PatientsController(VetoProDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: api/patients
    /// Récupère la liste de tous les patients avec leurs détails.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients()
    {
        var patients = await _context.Patients
            .Include(p => p.Owner) // Jointure avec Contacts (Propriétaire)
            .Include(p => p.Breed)
                .ThenInclude(b => b.Species) // Jointure imbriquée avec Species
            .Include(p => p.Colors) // Jointure avec Colors (M2M)
            .OrderBy(p => p.Name)
            .Select(p => MapToPatientDto(p)) // Utiliser la méthode de mapping
            .ToListAsync();

        return Ok(patients);
    }

    /// <summary>
    /// GET: api/patients/{id}
    /// Récupère un patient spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> GetPatientById(Guid id)
    {
        var patient = await _context.Patients
            .Include(p => p.Owner)
            .Include(p => p.Breed)
                .ThenInclude(b => b.Species)
            .Include(p => p.Colors)
            .Where(p => p.Id == id)
            .Select(p => MapToPatientDto(p))
            .FirstOrDefaultAsync();

        if (patient == null)
        {
            return NotFound("Patient non trouvé.");
        }

        return Ok(patient);
    }

    /// <summary>
    /// POST: api/patients
    /// Crée un nouveau patient.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] PatientCreateDto createDto)
    {
        // 1. Valider les clés étrangères (et récupérer les entités)
        
        var owner = await _context.Contacts.FindAsync(createDto.OwnerId);
        if (owner == null)
        {
            return BadRequest("L'ID du propriétaire (OwnerId) n'existe pas.");
        }
        
        var breed = await _context.Breeds.Include(b => b.Species).FirstOrDefaultAsync(b => b.Id == createDto.BreedId);
        if (breed == null)
        {
            return BadRequest("L'ID de la race (BreedId) n'existe pas.");
        }

        // 2. Valider et récupérer les entités Couleurs (M2M)
        var colors = await _context.Colors
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

        _context.Patients.Add(newPatient);
        await _context.SaveChangesAsync();

        // 4. Mapper au DTO de retour
        newPatient.Owner = owner; // 'owner' est maintenant un 'Contact' non-null
        newPatient.Breed = breed; // 'breed' inclut déjà 'Species'
        
        var patientDto = MapToPatientDto(newPatient);

        return CreatedAtAction(nameof(GetPatientById), new { id = patientDto.Id }, patientDto);
    }

    /// <summary>
    /// PUT: api/patients/{id}
    /// Met à jour un patient existant.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] PatientUpdateDto updateDto)
    {
        // 1. Trouver le patient et ses couleurs actuelles
        var patientToUpdate = await _context.Patients
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patientToUpdate == null)
        {
            return NotFound("Patient non trouvé.");
        }

        // 2. Valider les clés étrangères
        if (updateDto.OwnerId != patientToUpdate.OwnerId && !await _context.Contacts.AnyAsync(c => c.Id == updateDto.OwnerId))
        {
            return BadRequest("Le nouvel ID du propriétaire (OwnerId) n'existe pas.");
        }
        if (updateDto.BreedId != patientToUpdate.BreedId && !await _context.Breeds.AnyAsync(b => b.Id == updateDto.BreedId))
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
        var newColors = await _context.Colors
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
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Patients.Any(p => p.Id == id))
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
    public async Task<IActionResult> DeletePatient(Guid id)
    {
        var patientToDelete = await _context.Patients
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

        _context.Patients.Remove(patientToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    /// <summary>
    /// Méthode privée pour mapper une entité Patient vers un PatientDto.
    /// S'attend à ce que p.Owner, p.Breed.Species, et p.Colors soient pré-chargés (Included).
    /// </summary>
    private static PatientDto MapToPatientDto(Patient p)
    {
        return new PatientDto
        {
            Id = p.Id,
            Name = p.Name,
            ChipNumber = p.ChipNumber,
            DobEstimateStart = p.DobEstimateStart,
            DobEstimateEnd = p.DobEstimateEnd,
            Gender = p.Gender,
            ReproductiveStatus = p.ReproductiveStatus,
            DeceasedAt = p.DeceasedAt,
            
            // Mapper le Propriétaire
            OwnerId = p.OwnerId,
            OwnerFullName = $"{p.Owner.FirstName} {p.Owner.LastName}",
            
            // Mapper la Race
            BreedId = p.BreedId,
            BreedName = p.Breed.Name,
            
            // Mapper l'Espèce (via la Race)
            SpeciesId = p.Breed.SpeciesId,
            SpeciesName = p.Breed.Species.Name,

            // Mapper les Couleurs
            Colors = p.Colors.Select(c => new ColorDto
            {
                Id = c.Id,
                Name = c.Name,
                HexValue = c.HexValue
            }).ToList()
        };
    }
}