using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using System.Security.Claims;
using VetoPro.Api.Mapping;

namespace VetoPro.Api.Controllers;

[Authorize]
public class PaymentsController(VetoProDbContext context) : BaseApiController(context)
{
    /// <summary>
    /// GET: api/payments/{id}
    /// Récupère un paiement spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return NotFound("Paiement non trouvé.");
        }

        var (userContactId, errorResult) = await GetUserContactId();
        if (errorResult != null) return errorResult;

        bool isAdmin = User.IsInRole("Admin");
        bool isDoctor = User.IsInRole("Doctor");
        bool isOwnerClient = payment.Invoice.ClientId == userContactId;

        if (!isAdmin && !isDoctor && !isOwnerClient)
        {
            return Forbid(); // 403 Forbidden
        }
        
        return Ok(payment.ToDto());
    }

    /// <summary>
    /// DELETE: api/payments/{id}
    /// Supprime (annule) un paiement.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> DeletePayment(Guid id)
    {
        var paymentToDelete = await _context.Payments
            .Include(p => p.Invoice) // Charger la facture liée
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paymentToDelete == null)
        {
            return NotFound("Paiement non trouvé.");
        }

        var invoice = paymentToDelete.Invoice;

        _context.Payments.Remove(paymentToDelete);

        // Mettre à jour le statut de la facture (exemple de logique métier)
        if (invoice != null)
        {
            // Recalculer le total payé *après* suppression
            var totalPaid = await _context.Payments
                .Where(p => p.InvoiceId == invoice.Id && p.Id != id)
                .SumAsync(p => p.Amount);

            if (totalPaid < invoice.TotalAmount)
            {
                // Si la facture n'est plus soldée, on la repasse en "Envoyée" (ou "Draft")
                if (invoice.Status == "Paid")
                {
                    invoice.Status = "Sent"; 
                }
            }
        }

        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }
}