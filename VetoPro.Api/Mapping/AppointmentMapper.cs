using VetoPro.Api.DTOs;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Mapping;

public static class AppointmentMapper
{
    /// <summary>
    /// Maps an Appointment entity to an AppointmentDto.
    /// Assumes related entities (Client, Patient, Doctor) are loaded.
    /// </summary>
    public static AppointmentDto ToDto(this Appointment a)
    {
        // Handle cases where navigation properties might be null
        // (Client and Patient should always be loaded based on FK constraints)
        var clientName = (a.Client != null) ? $"{a.Client.FirstName} {a.Client.LastName}" : "N/A";
        var patientName = (a.Patient != null) ? a.Patient.Name : "N/A";
        // Doctor is optional
        var doctorName = (a.Doctor != null) ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : null;

        return new AppointmentDto
        {
            Id = a.Id,
            StartAt = a.StartAt, // EF Core gère la conversion UTC -> Local si nécessaire
            EndAt = a.EndAt,
            Reason = a.Reason,
            Notes = a.Notes,
            Status = a.Status,
            ClientId = a.ClientId,
            ClientName = clientName,
            PatientId = a.PatientId,
            PatientName = patientName,
            DoctorId = a.DoctorContactId,
            DoctorName = doctorName
        };
    }
}