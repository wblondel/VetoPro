using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Contracts.DTOs;
using VetoPro.Api.Mapping;

namespace VetoPro.Api.Controllers;

[Authorize]
public class ConsultationsController(VetoProDbContext context) : BaseApiController(context)
{
    /// <summary>
    /// GET: api/consultations/{id}
    /// Récupère une consultation (compte-rendu) par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConsultationDto>> GetConsultationById(Guid id)
    {
        var consultation = await _context.Consultations
            .Include(c => c.Client)
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .Include(c => c.Appointment)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (consultation == null)
        {
            return NotFound("Consultation non trouvée.");
        }
        
        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;

        bool isAdmin = User.IsInRole("Admin");
        
        // Le docteur peut être celui qui *a fait* la consult, ou celui qui était *assigné* au RDV
        bool isDoctor = User.IsInRole("Doctor") && 
                        (consultation.DoctorId == userContactId || consultation.Appointment?.DoctorContactId == userContactId); 
        
        bool isOwnerClient = consultation.ClientId == userContactId;

        if (!isAdmin && !isDoctor && !isOwnerClient)
        {
            return Forbid(); // 403 Forbidden
        }
        
        return Ok(consultation.ToDto());
    }

    /// <summary>
    /// PUT: api/consultations/{id}
    /// Met à jour un compte-rendu de consultation existant.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdateConsultation(Guid id, [FromBody] ConsultationUpdateDto updateDto)
    {
        var consultationToUpdate = await _context.Consultations.FindAsync(id);

        if (consultationToUpdate == null)
        {
            return NotFound("Consultation non trouvée.");
        }

        // Valider le docteur
        if (updateDto.DoctorId != consultationToUpdate.DoctorId &&
            !await _context.Contacts.AnyAsync(c => c.Id == updateDto.DoctorId && c.IsStaff))
        {
            return BadRequest("L'ID du docteur n'est pas valide ou n'est pas un membre du personnel.");
        }

        // Appliquer les modifications
        consultationToUpdate.DoctorId = updateDto.DoctorId;
        consultationToUpdate.ConsultationDate = updateDto.ConsultationDate.ToUniversalTime();
        consultationToUpdate.WeightKg = updateDto.WeightKg;
        consultationToUpdate.TemperatureCelsius = updateDto.TemperatureCelsius;
        consultationToUpdate.ClinicalExam = updateDto.ClinicalExam;
        consultationToUpdate.Diagnosis = updateDto.Diagnosis;
        consultationToUpdate.Treatment = updateDto.Treatment;
        consultationToUpdate.Prescriptions = updateDto.Prescriptions;
        // UpdatedAt sera géré par le DbContext

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/consultations/{id}
    /// Supprime un compte-rendu de consultation.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConsultation(Guid id)
    {
        var consultationToDelete = await _context.Consultations
            .Include(c => c.Invoices) // Vérifier les factures liées
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultationToDelete == null)
        {
            return NotFound("Consultation non trouvée.");
        }

        // Règle métier : Ne pas supprimer une consultation si elle est liée à une facture.
        if (consultationToDelete.Invoices.Any())
        {
            return BadRequest("Cette consultation ne peut pas être supprimée car elle est liée à une ou plusieurs factures.");
        }

        _context.Consultations.Remove(consultationToDelete);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}