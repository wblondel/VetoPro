using VetoPro.Contracts.DTOs;
using VetoPro.Api.Entities;
using VetoPro.Contracts.DTOs.Clinical;

namespace VetoPro.Api.Mapping;

public static class ConsultationMapper
{
    /// <summary>
    /// Maps a Consultation entity to a ConsultationDto.
    /// Assumes related entities (Client, Patient, Doctor) are loaded.
    /// </summary>
    public static ConsultationDto ToDto(this Consultation c)
    {
        // Handle cases where navigation properties might be null
        // although in our current secure controllers, they should be loaded.
        var clientName = (c.Client != null) ? $"{c.Client.FirstName} {c.Client.LastName}" : "N/A";
        var patientName = (c.Patient != null) ? c.Patient.Name : "N/A";
        var doctorName = (c.Doctor != null) ? $"{c.Doctor.FirstName} {c.Doctor.LastName}" : "N/A";

        return new ConsultationDto
        {
            Id = c.Id,
            AppointmentId = c.AppointmentId,
            ConsultationDate = c.ConsultationDate,
            ClientId = c.ClientId,
            ClientName = clientName,
            PatientId = c.PatientId,
            PatientName = patientName,
            DoctorId = c.DoctorId,
            DoctorName = doctorName,
            WeightKg = c.WeightKg,
            TemperatureCelsius = c.TemperatureCelsius,
            ClinicalExam = c.ClinicalExam,
            Diagnosis = c.Diagnosis,
            Treatment = c.Treatment,
            Prescriptions = c.Prescriptions
        };
    }
}