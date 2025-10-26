using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Controllers;

[Authorize]
public class AppointmentsController(VetoProDbContext context) : BaseApiController(context)
{
    /// <summary>
    /// GET: api/appointments
    /// Récupère la liste des rendez-vous.
    /// - Admin : Voit tout.
    /// - Doctor : Voit ses RDV assignés.
    /// - Client : Voit ses RDV.
    /// Permet de filtrer par plage de dates (ex: ?startDate=2025-10-01&endDate=2025-10-31).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        // Récupérer l'ID du contact de l'utilisateur connecté
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;
        
        // Commencer la requête de base
        var query = _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Doctor) // Le docteur est optionnel, Include gère le 'null'
            .OrderBy(a => a.StartAt)
            .AsQueryable(); // Rendre "queryable" pour ajouter des filtres
        
        if (User.IsInRole("Admin"))
        {
            // L'Admin voit tout, aucun filtre
        }
        else if (User.IsInRole("Doctor"))
        {
            // Le Docteur voit ses RDV assignés
            query = query.Where(a => a.DoctorContactId == userContactId);
        }
        else // "Client"
        {
            // Le Client voit ses RDV
            query = query.Where(a => a.ClientId == userContactId);
        }
        
        // Appliquer les filtres de date s'ils sont fournis
        if (startDate != null)
        {
            // On veut les RDV qui se terminent *après* le début de la plage
            query = query.Where(a => a.EndAt >= startDate);
        }
        if (endDate != null)
        {
            // On veut les RDV qui commencent *avant* la fin de la plage
            query = query.Where(a => a.StartAt <= endDate);
        }

        // Exécuter la requête finale et mapper les résultats en DTO
        var appointments = await query
            .Select(a => MapToAppointmentDto(a))
            .ToListAsync();

        return Ok(appointments);
    }

    /// <summary>
    /// GET: api/appointments/{id}
    /// Récupère un rendez-vous spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointmentById(Guid id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound("Rendez-vous non trouvé.");
        }
        
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;
        
        bool isAdmin = User.IsInRole("Admin");
        bool isAssignedDoctor = User.IsInRole("Doctor") && appointment.DoctorContactId == userContactId;
        bool isOwnerClient = appointment.ClientId == userContactId;
        
        if (!isAdmin && !isAssignedDoctor && !isOwnerClient)
        {
            return Forbid(); // 403 Forbidden
        }
        
        return Ok(MapToAppointmentDto(appointment));
    }

    /// <summary>
    /// POST: api/appointments
    /// Crée un nouveau rendez-vous.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] AppointmentCreateDto createDto)
    {
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;
        
        // Si l'utilisateur n'est PAS du staff, on force le ClientId à être le sien.
        if (!User.IsInRole("Admin") && !User.IsInRole("Doctor"))
        {
            if (createDto.ClientId != userContactId)
            {
                return Forbid("Vous ne pouvez créer des rendez-vous que pour vous-même.");
            }
        }
        
        // 1. Valider la logique des dates
        if (createDto.StartAt >= createDto.EndAt)
        {
            return BadRequest("L'heure de début doit être antérieure à l'heure de fin.");
        }

        // 2. Valider les clés étrangères
        if (!await _context.Contacts.AnyAsync(c => c.Id == createDto.ClientId && c.IsClient))
        {
            return BadRequest("L'ID du client (ClientId) n'existe pas ou n'est pas un client.");
        }
        if (!await _context.Patients.AnyAsync(p => p.Id == createDto.PatientId))
        {
            return BadRequest("L'ID du patient (PatientId) n'existe pas.");
        }
        if (createDto.DoctorContactId.HasValue &&
            !await _context.Contacts.AnyAsync(c => c.Id == createDto.DoctorContactId.Value && c.IsStaff))
        {
            return BadRequest("L'ID du docteur (DoctorContactId) n'existe pas ou n'est pas un membre du personnel.");
        }

        // 3. Mapper le DTO vers l'Entité
        var newAppointment = new Appointment
        {
            StartAt = createDto.StartAt.ToUniversalTime(), // Toujours stocker en UTC
            EndAt = createDto.EndAt.ToUniversalTime(),
            ClientId = createDto.ClientId,
            PatientId = createDto.PatientId,
            DoctorContactId = createDto.DoctorContactId,
            Reason = createDto.Reason,
            Notes = createDto.Notes,
            Status = createDto.Status
        };

        _context.Appointments.Add(newAppointment);
        await _context.SaveChangesAsync();

        // 4. Mapper en retour pour la réponse 201
        // Nous devons recharger l'entité pour obtenir les objets navigués (Client, Patient, etc.)
        var createdAppointment = await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstAsync(a => a.Id == newAppointment.Id);

        return CreatedAtAction(
            nameof(GetAppointmentById),
            new { id = createdAppointment.Id },
            MapToAppointmentDto(createdAppointment)
        );
    }

    /// <summary>
    /// PUT: api/appointments/{id}
    /// Met à jour un rendez-vous existant.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] AppointmentUpdateDto updateDto)
    {
        var appointmentToUpdate = await _context.Appointments.FindAsync(id);

        if (appointmentToUpdate == null)
        {
            return NotFound("Rendez-vous non trouvé.");
        }
        
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;

        bool isAdmin = User.IsInRole("Admin");
        bool isAssignedDoctor = User.IsInRole("Doctor") && appointmentToUpdate.DoctorContactId == userContactId;
        bool isOwnerClient = appointmentToUpdate.ClientId == userContactId;

        if (!isAdmin && !isAssignedDoctor && !isOwnerClient)
        {
            return Forbid("Vous n'avez pas l'autorisation de modifier ce rendez-vous.");
        }
        
        // Un client ne peut pas réassigner le RDV à quelqu'un d'autre
        if (!isAdmin && !isAssignedDoctor && updateDto.ClientId != userContactId)
        {
            return Forbid("Vous ne pouvez pas assigner ce rendez-vous à un autre client.");
        }
        
        // 1. Valider la logique des dates
        if (updateDto.StartAt >= updateDto.EndAt)
        {
            return BadRequest("L'heure de début doit être antérieure à l'heure de fin.");
        }

        // 2. Valider les clés étrangères (seulement si elles changent)
        if (updateDto.ClientId != appointmentToUpdate.ClientId &&
            !await _context.Contacts.AnyAsync(c => c.Id == updateDto.ClientId && c.IsClient))
        {
            return BadRequest("Le nouvel ID client n'existe pas ou n'est pas un client.");
        }
        if (updateDto.PatientId != appointmentToUpdate.PatientId &&
            !await _context.Patients.AnyAsync(p => p.Id == updateDto.PatientId))
        {
            return BadRequest("Le nouvel ID patient n'existe pas.");
        }
        if (updateDto.DoctorContactId != appointmentToUpdate.DoctorContactId &&
            updateDto.DoctorContactId.HasValue &&
            !await _context.Contacts.AnyAsync(c => c.Id == updateDto.DoctorContactId.Value && c.IsStaff))
        {
            return BadRequest("Le nouvel ID docteur n'existe pas ou n'est pas un membre du personnel.");
        }
        
        // 3. Appliquer les modifications
        appointmentToUpdate.StartAt = updateDto.StartAt.ToUniversalTime();
        appointmentToUpdate.EndAt = updateDto.EndAt.ToUniversalTime();
        appointmentToUpdate.ClientId = updateDto.ClientId;
        appointmentToUpdate.PatientId = updateDto.PatientId;
        appointmentToUpdate.DoctorContactId = updateDto.DoctorContactId;
        appointmentToUpdate.Reason = updateDto.Reason;
        appointmentToUpdate.Notes = updateDto.Notes;
        appointmentToUpdate.Status = updateDto.Status;
        // UpdatedAt sera géré par le DbContext

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/appointments/{id}
    /// Supprime un rendez-vous.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var appointmentToDelete = await _context.Appointments
            .Include(a => a.Consultation) // Charger la consultation liée
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointmentToDelete == null)
        {
            return NotFound("Rendez-vous non trouvé.");
        }

        // Règle métier : Ne pas supprimer un RDV s'il est lié à une consultation.
        // Il doit être "Annulé" (via PUT) mais pas supprimé pour garder l'historique.
        if (appointmentToDelete.Consultation != null)
        {
            return BadRequest("Ce rendez-vous ne peut pas être supprimé car il est lié à une consultation. Veuillez plutôt changer son statut en 'Annulé'.");
        }

        _context.Appointments.Remove(appointmentToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// GET: api/appointments/{id}/consultation
    /// Récupère la consultation (unique) associée à cet ID de rendez-vous.
    /// </summary>
    [HttpGet("{id}/consultation")]
    public async Task<ActionResult<ConsultationDto>> GetConsultationForAppointment(Guid id)
    {
        // On vérifie d'abord si le RDV existe
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound("Rendez-vous non trouvé.");
        
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;

        bool isAdmin = User.IsInRole("Admin");
        bool isAssignedDoctor = User.IsInRole("Doctor") && appointment.DoctorContactId == userContactId;
        bool isOwnerClient = appointment.ClientId == userContactId;

        if (!isAdmin && !isAssignedDoctor && !isOwnerClient)
        {
            return Forbid();
        }
        
        var consultation = await _context.Consultations
            .Include(c => c.Client)
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .Where(c => c.AppointmentId == id)
            .Select(c => MapToConsultationDto(c))
            .FirstOrDefaultAsync(); // Une seule consultation par RDV

        if (consultation == null)
        {
            return NotFound("Aucune consultation n'a encore été créée pour ce rendez-vous.");
        }

        return Ok(consultation);
    }
    
    /// <summary>
    /// POST: api/appointments/{id}/consultation
    /// Crée un compte-rendu de consultation pour un rendez-vous existant.
    /// </summary>
    [HttpPost("{id}/consultation")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<ConsultationDto>> CreateConsultationForAppointment(Guid id, [FromBody] ConsultationCreateDto createDto)
    {
        // 1. Trouver le rendez-vous parent
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound("Le rendez-vous (AppointmentId) n'a pas été trouvé.");
        }

        // 2. Vérifier si une consultation existe déjà (relation 1-à-1)
        if (await _context.Consultations.AnyAsync(c => c.AppointmentId == id))
        {
            return Conflict("Ce rendez-vous a déjà un compte-rendu de consultation.");
        }

        // 3. Valider le docteur
        var doctor = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == createDto.DoctorId && c.IsStaff);
        if (doctor == null)
        {
            return BadRequest("L'ID du docteur n'est pas valide ou n'est pas un membre du personnel.");
        }

        // 4. Récupérer le patient et le client (pour le DTO de retour)
        var patient = await _context.Patients.FindAsync(appointment.PatientId);
        var client = await _context.Contacts.FindAsync(appointment.ClientId);
        // Ces vérifications ne sont pas nécessaires car l'intégrité de la BDD garantit
        // qu'ils existent si le RDV existe.

        // 5. Créer la nouvelle entité Consultation
        var newConsultation = new Consultation
        {
            AppointmentId = id, // Lier au RDV parent
            
            // Copier les IDs depuis le RDV parent
            PatientId = appointment.PatientId,
            ClientId = appointment.ClientId, 
            
            // Prendre les infos du DTO
            DoctorId = createDto.DoctorId,
            ConsultationDate = createDto.ConsultationDate.ToUniversalTime(),
            WeightKg = createDto.WeightKg,
            TemperatureCelsius = createDto.TemperatureCelsius,
            ClinicalExam = createDto.ClinicalExam,
            Diagnosis = createDto.Diagnosis,
            Treatment = createDto.Treatment,
            Prescriptions = createDto.Prescriptions
        };

        _context.Consultations.Add(newConsultation);

        // 6. Mettre à jour le statut du RDV (bonne pratique)
        appointment.Status = "Completed";

        await _context.SaveChangesAsync();

        // 7. Mapper et renvoyer le DTO
        // Nous attachons manuellement les objets navigués pour le mapping
        newConsultation.Patient = patient!;
        newConsultation.Client = client!;
        newConsultation.Doctor = doctor;

        var consultationDto = MapToConsultationDto(newConsultation);

        // Renvoie une URL vers le endpoint GetById du *ConsultationsController*
        return CreatedAtAction(
            "GetConsultationById", // Nom de l'action
            "Consultations",       // Nom du contrôleur
            new { id = consultationDto.Id }, // Paramètres de route
            consultationDto
        );
    }
    
    /// <summary>
    /// Méthode privée pour mapper une entité Appointment vers un AppointmentDto.
    /// S'attend à ce que a.Client, a.Patient, et a.Doctor soient pré-chargés.
    /// </summary>
    private static AppointmentDto MapToAppointmentDto(Appointment a)
    {
        return new AppointmentDto
        {
            Id = a.Id,
            StartAt = a.StartAt, // EF Core gère la conversion UTC -> Local si nécessaire
            EndAt = a.EndAt,
            Reason = a.Reason,
            Notes = a.Notes,
            Status = a.Status,
            ClientId = a.ClientId,
            ClientName = $"{a.Client.FirstName} {a.Client.LastName}",
            PatientId = a.PatientId,
            PatientName = a.Patient.Name,
            DoctorId = a.DoctorContactId,
            // Gérer le docteur optionnel
            DoctorName = a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : null
        };
    }
    
    /// <summary>
    /// Méthode privée (dupliquée) pour mapper Consultation vers ConsultationDto.
    /// S'attend à ce que c.Client, c.Patient, et c.Doctor soient pré-chargés.
    /// </summary>
    private static ConsultationDto MapToConsultationDto(Consultation c)
    {
        return new ConsultationDto
        {
            Id = c.Id,
            AppointmentId = c.AppointmentId,
            ConsultationDate = c.ConsultationDate,
            ClientId = c.ClientId,
            ClientName = $"{c.Client.FirstName} {c.Client.LastName}",
            PatientId = c.PatientId,
            PatientName = c.Patient.Name,
            DoctorId = c.DoctorId,
            DoctorName = $"{c.Doctor.FirstName} {c.Doctor.LastName}",
            WeightKg = c.WeightKg,
            TemperatureCelsius = c.TemperatureCelsius,
            ClinicalExam = c.ClinicalExam,
            Diagnosis = c.Diagnosis,
            Treatment = c.Treatment,
            Prescriptions = c.Prescriptions
        };
    }
}