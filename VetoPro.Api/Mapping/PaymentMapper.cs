using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using VetoPro.Contracts.DTOs.Financials;

namespace VetoPro.Api.Mapping;

public static class PaymentMapper
{
    /// <summary>
    /// Maps a Payment entity to a PaymentDto.
    /// </summary>
    public static PaymentDto ToDto(this Payment p)
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