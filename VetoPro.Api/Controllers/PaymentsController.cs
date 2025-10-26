using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/payments
public class PaymentsController : ControllerBase
{
    private readonly VetoProDbContext _context;

    public PaymentsController(VetoProDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: api/payments/{id}
    /// Récupère un paiement spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id)
    {
        var payment = await _context.Payments
            .Where(p => p.Id == id)
            .Select(p => MapToPaymentDto(p))
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return NotFound("Paiement non trouvé.");
        }

        return Ok(payment);
    }

    /// <summary>
    /// DELETE: api/payments/{id}
    /// Supprime (annule) un paiement.
    /// </summary>
    [HttpDelete("{id}")]
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
    
    /// <summary>
    /// Méthode privée pour mapper une entité Payment vers un PaymentDto.
    /// </summary>
    private static PaymentDto MapToPaymentDto(Payment p)
    {
        return new PaymentDto
        {
            Id = p.Id,
            InvoiceId = p.InvoiceId,
            PaymentDate = p.PaymentDate,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            TransactionId = p.TransactionId
        };
    }
}