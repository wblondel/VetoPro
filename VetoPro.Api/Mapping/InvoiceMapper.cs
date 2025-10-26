using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class InvoiceMapper
{
    /// <summary>
    /// Maps an Invoice entity to an InvoiceDto.
    /// Assumes related entities (Client, InvoiceLines, Payments) are loaded.
    /// </summary>
    public static InvoiceDto ToDto(this Invoice i)
    {
        // Handle potential null navigation properties
        var clientName = (i.Client != null) ? $"{i.Client.FirstName} {i.Client.LastName}" : "N/A";

        return new InvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            IssueDate = i.IssueDate,
            DueDate = i.DueDate,
            TotalAmount = i.TotalAmount,
            Status = i.Status,
            ClientId = i.ClientId,
            ClientName = clientName,
            ConsultationId = i.ConsultationId,

            // Map Invoice Lines
            InvoiceLines = i.InvoiceLines?.Select(line => new InvoiceLineDto
            {
                Id = line.Id,
                ItemId = line.ServiceId ?? line.ProductId,
                ItemType = line.ServiceId.HasValue ? "Service" : "Product",
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineTotal = line.LineTotal
            }).ToList() ?? new List<InvoiceLineDto>(), // Handle null InvoiceLines

            // Calculate Amount Paid
            AmountPaid = i.Payments?.Sum(p => p.Amount) ?? 0m // Handle null Payments and default to 0
        };
    }
}