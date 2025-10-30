using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Entities;
using VetoPro.Api.Helpers;
using VetoPro.Api.Mapping;
using VetoPro.Contracts.DTOs.Financials;
using FluentValidation;

namespace VetoPro.Api.Controllers.Financials;

[Authorize]
public class InvoicesController : BaseApiController
{
    private readonly IValidator<InvoiceCreateDto> _invoiceCreateValidator;
    private readonly IValidator<InvoiceUpdateDto> _invoiceUpdateValidator;
    private readonly IValidator<PaymentCreateDto> _paymentCreateValidator;
    
    public InvoicesController(
        VetoProDbContext context,
        IValidator<InvoiceCreateDto> invoiceCreateValidator,
        IValidator<InvoiceUpdateDto> invoiceUpdateValidator,
        IValidator<PaymentCreateDto> paymentCreateValidator) : base(context)
    {
        _invoiceCreateValidator = invoiceCreateValidator;
        _invoiceUpdateValidator = invoiceUpdateValidator;
        _paymentCreateValidator = paymentCreateValidator;
    }

    /// <summary>
    /// GET: api/invoices
    /// Récupère la liste de toutes les factures (sans les lignes de détail).
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    [HttpGet]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAllInvoices([FromQuery] PaginationParams paginationParams)
    {
        // Note : Ce DTO est partiel (sans les lignes) pour la performance.
        // Un DTO "InvoiceSummaryDto" serait idéal, mais nous réutilisons InvoiceDto.
        var query = _context.Invoices
            .Include(i => i.Client)
            .OrderByDescending(i => i.IssueDate)
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, i => i.ToDto());
    }

    /// <summary>
    /// GET: api/invoices/{id}
    /// Récupère une facture complète (avec lignes et total payé) par son ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceById(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.InvoiceLines) // Charger les lignes
            .Include(i => i.Payments) // Charger les paiements
            .Where(i => i.Id == id)
            // On ne map pas toute de suite, on a besoin de l'entité pour la vérification
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return NotFound("Facture non trouvée.");
        }

        if (!User.IsInRole("Admin") && !User.IsInRole("Doctor"))
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userContact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId.ToString() == currentUserId);

            if (invoice.Client.Id != userContact?.Id)
            {
                return Forbid();
            }
        }
        
        var invoiceDto = invoice.ToDto();
        return Ok(invoiceDto);
    }

    /// <summary>
    /// POST: api/invoices
    /// Crée une nouvelle facture et ses lignes de détail.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] InvoiceCreateDto createDto)
    {
        var validationResult = await _invoiceCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Utiliser une transaction pour garantir que la facture ET ses lignes sont créées, ou rien.
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Valider les clés étrangères (Client, Consultation)
            if (!await _context.Contacts.AnyAsync(c => c.Id == createDto.ClientId))
            {
                return BadRequest("L'ID du client (ClientId) n'existe pas.");
            }
            if (createDto.ConsultationId.HasValue && !await _context.Consultations.AnyAsync(c => c.Id == createDto.ConsultationId.Value))
            {
                return BadRequest("L'ID de la consultation (ConsultationId) n'existe pas.");
            }
            
            // 2. Valider le numéro de facture (contrainte unique)
            if (await _context.Invoices.AnyAsync(i => i.InvoiceNumber == createDto.InvoiceNumber))
            {
                return Conflict("Ce numéro de facture (InvoiceNumber) est déjà utilisé.");
            }

            decimal calculatedTotal = 0;
            var newInvoiceLines = new List<InvoiceLine>();

            // 3. Valider et préparer les lignes de facture
            foreach (var lineDto in createDto.InvoiceLines)
            {
                string description;
                // Valider l'article (Service ou Produit)
                if (lineDto.ItemType.Equals("Service", StringComparison.OrdinalIgnoreCase))
                {
                    var service = await _context.Services.FindAsync(lineDto.ItemId);
                    if (service == null) return BadRequest($"La ligne de service avec ItemId {lineDto.ItemId} n'existe pas.");
                    description = service.Name;
                }
                else if (lineDto.ItemType.Equals("Product", StringComparison.OrdinalIgnoreCase))
                {
                    var product = await _context.Products.FindAsync(lineDto.ItemId);
                    if (product == null) return BadRequest($"La ligne de produit avec ItemId {lineDto.ItemId} n'existe pas.");
                    description = product.Name;
                    // TODO: Gérer la déduction du stock (product.StockQuantity -= lineDto.Quantity)
                }
                else
                {
                    return BadRequest($"Type d'article (ItemType) non valide : '{lineDto.ItemType}'.");
                }

                var lineTotal = lineDto.Quantity * lineDto.UnitPrice;
                calculatedTotal += lineTotal;

                newInvoiceLines.Add(new InvoiceLine
                {
                    ServiceId = lineDto.ItemType.Equals("Service") ? lineDto.ItemId : null,
                    ProductId = lineDto.ItemType.Equals("Product") ? lineDto.ItemId : null,
                    Description = description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    LineTotal = lineTotal
                });
            }

            // 4. Créer l'entité Facture (Invoice)
            var newInvoice = new Invoice
            {
                ClientId = createDto.ClientId,
                ConsultationId = createDto.ConsultationId,
                InvoiceNumber = createDto.InvoiceNumber,
                IssueDate = createDto.IssueDate,
                DueDate = createDto.DueDate,
                Status = createDto.Status,
                TotalAmount = calculatedTotal,
                InvoiceLines = newInvoiceLines // Attacher les lignes
            };

            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 5. Recharger et renvoyer le DTO complet
            var createdInvoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceLines)
                .Include(i => i.Payments)
                .FirstAsync(i => i.Id == newInvoice.Id);

            return CreatedAtAction(nameof(GetInvoiceById), new { id = createdInvoice.Id }, createdInvoice.ToDto());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erreur interne lors de la création de la facture: {ex.Message}");
        }
    }

    /// <summary>
    /// PUT: api/invoices/{id}
    /// Met à jour une facture et synchronise ses lignes de détail.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] InvoiceUpdateDto updateDto)
    {
        var validationResult = await _invoiceUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var invoiceToUpdate = await _context.Invoices
                .Include(i => i.InvoiceLines) // Charger les lignes existantes
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoiceToUpdate == null)
            {
                return NotFound("Facture non trouvée.");
            }
            
            // Règle métier : Ne pas modifier une facture si elle est déjà payée ou annulée
            if (invoiceToUpdate.Status == "Paid" || invoiceToUpdate.Status == "Void")
            {
                return BadRequest($"La facture ne peut pas être modifiée car son statut est '{invoiceToUpdate.Status}'.");
            }
            
            if (await _context.Invoices.AnyAsync(i => i.InvoiceNumber == updateDto.InvoiceNumber && i.Id != id))
            {
                return Conflict("Ce numéro de facture (InvoiceNumber) est déjà utilisé par une autre facture.");
            }

            // 1. Valider les clés étrangères
            if (updateDto.ClientId != invoiceToUpdate.ClientId && !await _context.Contacts.AnyAsync(c => c.Id == updateDto.ClientId))
            {
                return BadRequest("Le nouvel ID client n'existe pas.");
            }
            // TODO: validation similaire pour ConsultationId, InvoiceNumber

            // 2. Synchroniser les lignes de facture (Logique "Upsert" + Suppression)
            var newLineDtosById = updateDto.InvoiceLines
                .Where(line => line.Id.HasValue)
                .ToDictionary(line => line.Id!.Value);
            var existingLines = invoiceToUpdate.InvoiceLines.ToList();
            
            // Supprimer les lignes qui ne sont plus dans le DTO
            foreach (var existingLine in existingLines)
            {
                if (!newLineDtosById.ContainsKey(existingLine.Id))
                {
                    _context.InvoiceLines.Remove(existingLine);
                }
            }

            decimal calculatedTotal = 0;
            // Mettre à jour les lignes existantes et ajouter les nouvelles
            foreach (var lineDto in updateDto.InvoiceLines)
            {
                string description;
                // ... (Validation de ItemId et ItemType comme dans CreateInvoice) ...
                if (lineDto.ItemType.Equals("Service", StringComparison.OrdinalIgnoreCase))
                {
                    var service = await _context.Services.FindAsync(lineDto.ItemId);
                    if (service == null) return BadRequest($"La ligne de service avec ItemId {lineDto.ItemId} n'existe pas.");
                    description = service.Name;
                }
                else
                {
                    var product = await _context.Products.FindAsync(lineDto.ItemId);
                    if (product == null) return BadRequest($"La ligne de produit avec ItemId {lineDto.ItemId} n'existe pas.");
                    description = product.Name;
                }
                
                var lineTotal = lineDto.Quantity * lineDto.UnitPrice;
                calculatedTotal += lineTotal;

                var existingLine = existingLines.FirstOrDefault(l => l.Id == lineDto.Id);
                if (existingLine != null)
                {
                    // Mettre à jour la ligne existante
                    existingLine.ServiceId = lineDto.ItemType.Equals("Service") ? lineDto.ItemId : null;
                    existingLine.ProductId = lineDto.ItemType.Equals("Product") ? lineDto.ItemId : null;
                    existingLine.Description = description;
                    existingLine.Quantity = lineDto.Quantity;
                    existingLine.UnitPrice = lineDto.UnitPrice;
                    existingLine.LineTotal = lineTotal;
                }
                else
                {
                    // Ajouter la nouvelle ligne
                    invoiceToUpdate.InvoiceLines.Add(new InvoiceLine
                    {
                        ServiceId = lineDto.ItemType.Equals("Service") ? lineDto.ItemId : null,
                        ProductId = lineDto.ItemType.Equals("Product") ? lineDto.ItemId : null,
                        Description = description,
                        Quantity = lineDto.Quantity,
                        UnitPrice = lineDto.UnitPrice,
                        LineTotal = lineTotal
                    });
                }
            }

            // 3. Mettre à jour l'en-tête de la facture
            invoiceToUpdate.ClientId = updateDto.ClientId;
            invoiceToUpdate.ConsultationId = updateDto.ConsultationId;
            invoiceToUpdate.InvoiceNumber = updateDto.InvoiceNumber;
            invoiceToUpdate.IssueDate = updateDto.IssueDate;
            invoiceToUpdate.DueDate = updateDto.DueDate;
            invoiceToUpdate.Status = updateDto.Status;
            invoiceToUpdate.TotalAmount = calculatedTotal;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erreur interne lors de la mise à jour de la facture: {ex.Message}");
        }
        
        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/invoices/{id}
    /// Supprime une facture.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        var invoiceToDelete = await _context.Invoices
            .Include(i => i.Payments)
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoiceToDelete == null)
        {
            return NotFound("Facture non trouvée.");
        }

        // Règle métier : Ne pas supprimer une facture qui a des paiements.
        if (invoiceToDelete.Payments.Any())
        {
            return BadRequest("Cette facture ne peut pas être supprimée car elle a des paiements associés. Veuillez d'abord annuler les paiements ou changer le statut de la facture en 'Annulé' (Void).");
        }
        
        // Règle métier : Ne pas supprimer une facture payée (Paid)
        if (invoiceToDelete.Status == "Paid")
        {
            return BadRequest("Une facture payée ne peut pas être supprimée.");
        }
        
        // La suppression en cascade (configurée dans le DbContext ou par défaut)
        // devrait supprimer les InvoiceLines. Mais nous pouvons être explicites.
        _context.InvoiceLines.RemoveRange(invoiceToDelete.InvoiceLines);
        _context.Invoices.Remove(invoiceToDelete);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    /// <summary>
    /// GET: api/invoices/{id}/payments
    /// Récupère la liste de tous les paiements pour une facture spécifique.
    /// </summary>
    [HttpGet("{id}/payments")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsForInvoice(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        //if (!await _context.Invoices.AnyAsync(i => i.Id == id))
        if (invoice == null)
        {
            return NotFound("Facture non trouvée.");
        }
        
        if (!User.IsInRole("Admin") && !User.IsInRole("Doctor"))
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userContact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId.ToString() == currentUserId);

            if (invoice.ClientId != userContact?.Id)
            {
                return Forbid();
            }
        }
        
        var payments = await _context.Payments
            .Where(p => p.InvoiceId == id)
            .OrderBy(p => p.PaymentDate)
            .Select(p => p.ToDto())
            .ToListAsync();

        return Ok(payments);
    }
    
    /// <summary>
    /// POST: api/invoices/{id}/payments
    /// Enregistre un nouveau paiement pour une facture.
    /// </summary>
    [HttpPost("{id}/payments")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<PaymentDto>> CreatePaymentForInvoice(Guid id, [FromBody] PaymentCreateDto createDto)
    {
        var validationResult = await _paymentCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Vérifier que le DTO est pour la bonne facture
        if (id != createDto.InvoiceId)
        {
            return BadRequest("L'ID de la facture dans l'URL ne correspond pas à l'ID dans le corps de la requête.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Payments) // Charger les paiements existants
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return NotFound("Facture non trouvée.");
        }

        // Règle métier : Ne pas ajouter de paiement à une facture annulée
        if (invoice.Status == "Void")
        {
            return BadRequest("Les paiements ne peuvent pas être ajoutés à une facture annulée (Void).");
        }

        // Règle métier : Gérer les trop-perçus
        var totalPaid = invoice.Payments.Sum(p => p.Amount);
        if (totalPaid + createDto.Amount > invoice.TotalAmount)
        {
            // Autoriser le trop-perçu, mais peut-être logger un avertissement.
            // Pour cet exemple, nous l'autorisons, mais nous nous assurons que le statut est "Paid".
        }
        
        // 1. Créer l'entité Paiement
        var newPayment = new Payment
        {
            InvoiceId = createDto.InvoiceId,
            PaymentDate = createDto.PaymentDate.ToUniversalTime(),
            Amount = createDto.Amount,
            PaymentMethod = createDto.PaymentMethod,
            TransactionId = createDto.TransactionId
        };

        _context.Payments.Add(newPayment);

        // 2. Mettre à jour le statut de la facture
        if (totalPaid + createDto.Amount >= invoice.TotalAmount)
        {
            invoice.Status = "Paid";
        }
        else
        {
            invoice.Status = "Sent"; // TODO: supporter les paiements partiels avec "PartiallyPaid" 
        }

        await _context.SaveChangesAsync();

        // 3. Mapper et renvoyer le DTO
        var paymentDto = newPayment.ToDto();

        // Renvoie une URL vers le endpoint GetById du *PaymentsController*
        return CreatedAtAction(
            "GetPaymentById",      // Nom de l'action
            "Payments",            // Nom du contrôleur
            new { id = paymentDto.Id }, // Paramètres de route
            paymentDto);
    }
}