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
public class AppointmentsController(
    VetoProDbContext context,
    IValidator<AppointmentCreateDto> appointmentCreateValidator,
    IValidator<AppointmentUpdateDto> appointmentUpdateValidator)
    : BaseApiController(context)
{
    /// <summary>
    /// GET: api/appointments
    /// Récupère la liste des rendez-vous.
    /// - Admin : Voit tout.
    /// - Doctor : Voit ses RDV assignés.
    /// - Client : Voit ses RDV.
    /// Permet de filtrer par plage de dates (ex: ?startDate=2025-10-01&endDate=2025-10-31).
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        // Récupérer l'ID du contact de l'utilisateur connecté
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;
        
        // Commencer la requête de base
        var query = Context.Appointments
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
        
        return await CreatePaginatedResponse(query, paginationParams, a => a.ToDto());
    }

    /// <summary>
    /// GET: api/appointments/{id}
    /// Récupère un rendez-vous spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointmentById(Guid id)
    {
        var appointment = await Context.Appointments
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
        
        return Ok(appointment.ToDto());
    }

    /// <summary>
    /// POST: api/appointments
    /// Crée un nouveau rendez-vous.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] AppointmentCreateDto createDto)
    {
        var validationResult = await appointmentCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            // FluentValidation populates ModelState, so this returns a detailed 400
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
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
        
        // 2. Valider les clés étrangères
        if (!await Context.Contacts.AnyAsync(c => c.Id == createDto.ClientId && c.IsClient))
        {
            return BadRequest("L'ID du client (ClientId) n'existe pas ou n'est pas un client.");
        }
        if (!await Context.Patients.AnyAsync(p => p.Id == createDto.PatientId))
        {
            return BadRequest("L'ID du patient (PatientId) n'existe pas.");
        }
        if (createDto.DoctorContactId.HasValue &&
            !await Context.Contacts.AnyAsync(c => c.Id == createDto.DoctorContactId.Value && c.IsStaff))
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

        Context.Appointments.Add(newAppointment);
        await Context.SaveChangesAsync();

        // 4. Mapper en retour pour la réponse 201
        // Nous devons recharger l'entité pour obtenir les objets navigués (Client, Patient, etc.)
        var createdAppointment = await Context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstAsync(a => a.Id == newAppointment.Id);

        return CreatedAtAction(
            nameof(GetAppointmentById),
            new { id = createdAppointment.Id },
            createdAppointment.ToDto()
        );
    }

    /// <summary>
    /// PUT: api/appointments/{id}
    /// Met à jour un rendez-vous existant.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] AppointmentUpdateDto updateDto)
    {
        var validationResult = await appointmentUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        var appointmentToUpdate = await Context.Appointments.FindAsync(id);

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

        // 2. Valider les clés étrangères (seulement si elles changent)
        if (updateDto.ClientId != appointmentToUpdate.ClientId &&
            !await Context.Contacts.AnyAsync(c => c.Id == updateDto.ClientId && c.IsClient))
        {
            return BadRequest("Le nouvel ID client n'existe pas ou n'est pas un client.");
        }
        if (updateDto.PatientId != appointmentToUpdate.PatientId &&
            !await Context.Patients.AnyAsync(p => p.Id == updateDto.PatientId))
        {
            return BadRequest("Le nouvel ID patient n'existe pas.");
        }
        if (updateDto.DoctorContactId != appointmentToUpdate.DoctorContactId &&
            updateDto.DoctorContactId.HasValue &&
            !await Context.Contacts.AnyAsync(c => c.Id == updateDto.DoctorContactId.Value && c.IsStaff))
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

        await Context.SaveChangesAsync();

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
        var appointmentToDelete = await Context.Appointments
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

        Context.Appointments.Remove(appointmentToDelete);
        await Context.SaveChangesAsync();

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
        var appointment = await Context.Appointments.FindAsync(id);
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
        
        var consultation = await Context.Consultations
            .Include(c => c.Client)
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .Where(c => c.AppointmentId == id)
            .Select(c => c.ToDto())
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
        var appointment = await Context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound("Le rendez-vous (AppointmentId) n'a pas été trouvé.");
        }

        // 2. Vérifier si une consultation existe déjà (relation 1-à-1)
        if (await Context.Consultations.AnyAsync(c => c.AppointmentId == id))
        {
            return Conflict("Ce rendez-vous a déjà un compte-rendu de consultation.");
        }

        // 3. Valider le docteur
        var doctor = await Context.Contacts.FirstOrDefaultAsync(c => c.Id == createDto.DoctorId && c.IsStaff);
        if (doctor == null)
        {
            return BadRequest("L'ID du docteur n'est pas valide ou n'est pas un membre du personnel.");
        }

        // 4. Récupérer le patient et le client (pour le DTO de retour)
        var patient = await Context.Patients.FindAsync(appointment.PatientId);
        var client = await Context.Contacts.FindAsync(appointment.ClientId);
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

        Context.Consultations.Add(newConsultation);

        // 6. Mettre à jour le statut du RDV (bonne pratique)
        appointment.Status = "Completed";

        await Context.SaveChangesAsync();

        // 7. Mapper et renvoyer le DTO
        // Nous attachons manuellement les objets navigués pour le mapping
        newConsultation.Patient = patient!;
        newConsultation.Client = client!;
        newConsultation.Doctor = doctor;

        var consultationDto = newConsultation.ToDto();

        // Renvoie une URL vers le endpoint GetById du *ConsultationsController*
        return CreatedAtAction(
            "GetConsultationById", // Nom de l'action
            "Consultations",       // Nom du contrôleur
            new { id = consultationDto.Id }, // Paramètres de route
            consultationDto
        );
    }
}