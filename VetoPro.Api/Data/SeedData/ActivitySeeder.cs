using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Data.SeedData;

public static class ActivitySeeder
{
    public static async Task SeedInitialPatients(VetoProDbContext context)
    {
        if (await context.Patients.AnyAsync()) return;

        // --- Récupérer les IDs nécessaires ---
        var dogBreed = await context.Breeds.FirstOrDefaultAsync(b => b.Name == "Labrador Retriever");
        var catBreed = await context.Breeds.FirstOrDefaultAsync(b => b.Name == "Européen" && b.Species.Name == "Chat");
        var whiteColor = await context.Colors.FirstOrDefaultAsync(c => c.Name == "Blanc");
        var blackColor = await context.Colors.FirstOrDefaultAsync(c => c.Name == "Noir");
        var clinicContact = await context.Contacts.FirstOrDefaultAsync(c => c.LastName == "VetoPro");

        // Créer/Récupérer le client
        var clientContact = await context.Contacts.FirstOrDefaultAsync(c => c.Email == "client.test@example.com");
        if (clientContact == null)
        {
            clientContact = new Contact
            {
                FirstName = "Julien",
                LastName = "Dupond",
                Email = "client.test@example.com",
                PhoneNumber = "0601010101",
                IsOwner = true,
                IsClient = true,
                IsStaff = false,
                // Assuming this client doesn't have an online account (UserId = null)
            };
            context.Contacts.Add(clientContact);
            await context.SaveChangesAsync();
        }

        if (dogBreed == null || catBreed == null || whiteColor == null || blackColor == null || clinicContact == null)
        {
            return;
        }

        var patients = new List<Patient>
        {
            // 1. Patient owned by the created client (Julien Dupond)
            new()
            {
                Name = "Max",
                OwnerId = clientContact.Id,
                BreedId = dogBreed.Id,
                ChipNumber = "981000001234567",
                DobEstimateStart = new DateOnly(2020, 5, 10),
                DobEstimateEnd = new DateOnly(2020, 5, 10),
                Gender = "Male",
                ReproductiveStatus = "Neutered",
                // M2M relationship requires the list of entities
                Colors = new List<Color> { blackColor }
            },
            // 2. Patient owned by the clinic (representing a stray/rescue)
            new()
            {
                Name = "Félix",
                OwnerId = clinicContact.Id,
                BreedId = catBreed.Id,
                ChipNumber = "981000007654321",
                DobEstimateStart = new DateOnly(2023, 1, 1),
                DobEstimateEnd = new DateOnly(2023, 3, 31), // Partial birthdate (first quarter 2023)
                Gender = "Female",
                ReproductiveStatus = "Intact",
                Colors = new List<Color> { whiteColor }
            }
        };

        await context.Patients.AddRangeAsync(patients);
        await context.SaveChangesAsync();
    }

    public static async Task SeedAppointments(VetoProDbContext context)
    {
        if (await context.Appointments.AnyAsync()) return;
        
        // --- Retrieve necessary entities/IDs ---
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@vetopro.com");
        var doctorContact = await context.Contacts.FirstOrDefaultAsync(c => c.UserId == adminUser!.Id);
        var clientContact = await context.Contacts.FirstOrDefaultAsync(c => c.Email == "client.test@example.com");
        var maxPatient =
            await context.Patients.FirstOrDefaultAsync(p => p.Name == "Max" && p.OwnerId == clientContact!.Id);
        var felixPatient = await context.Patients.FirstOrDefaultAsync(p => p.Name == "Félix");

        if (doctorContact == null || clientContact == null || maxPatient == null || felixPatient == null)
        {
            Console.WriteLine("ERROR: Cannot seed appointments due to missing contact or patient dependencies.");
            return;
        }

        // Use a fixed date for consistency (e.g., tomorrow's date)
        var today = DateTime.UtcNow.Date;

        var appointments = new List<Appointment>
        {
            // 1. Appointment for Max (Client Dupond) - Completed
            new()
            {
                ClientId = clientContact.Id,
                PatientId = maxPatient.Id,
                DoctorContactId = doctorContact.Id, // Assign to the Admin/Doctor
                StartAt = today.AddHours(9).ToUniversalTime(),
                EndAt = today.AddHours(9).AddMinutes(30).ToUniversalTime(),
                Reason = "Vaccination annuelle",
                Status = "Completed", // This one will have a consultation
            },
            // 2. Appointment for Félix (Clinic Pet) - Scheduled
            new()
            {
                ClientId = clientContact.Id, // Client is the one who took the pet in
                PatientId = felixPatient.Id,
                DoctorContactId = doctorContact.Id,
                StartAt = today.AddDays(1).AddHours(14).ToUniversalTime(), // Tomorrow
                EndAt = today.AddDays(1).AddHours(14).AddMinutes(30).ToUniversalTime(),
                Reason = "Contrôle post-sauvetage",
                Status = "Scheduled", // This one will NOT have a consultation yet
            }
        };

        await context.Appointments.AddRangeAsync(appointments);
        await context.SaveChangesAsync();
    }

    public static async Task SeedConsultations(VetoProDbContext context)
    {
        if (await context.Consultations.AnyAsync()) return;

        // --- Retrieve the completed appointment ---
        var completedAppointment = await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Client)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Status == "Completed");

        if (completedAppointment == null || completedAppointment.DoctorContactId == null)
        {
            Console.WriteLine(
                "ERROR: Cannot seed consultations because no completed appointment or doctor link was found.");
            return;
        }

        // Create the Consultation record
        var consultation = new Consultation
        {
            AppointmentId = completedAppointment.Id,
            PatientId = completedAppointment.PatientId,
            ClientId = completedAppointment.ClientId,
            DoctorId = completedAppointment.DoctorContactId.Value,
            ConsultationDate = completedAppointment.EndAt, // Use the appointment end time

            // Vitals
            WeightKg = 30.5m,
            TemperatureCelsius = 38.5m,

            // Report details
            ClinicalExam = "Examen clinique complet normal. Bon état général. Poids stable. Muqueuses roses.",
            Diagnosis = "Prévention annuelle.",
            Treatment = "Vaccination effectuée. Conseils nutritionnels donnés.",
            Prescriptions = "Rappel vermifuge dans 3 mois."
        };

        context.Consultations.Add(consultation);
        await context.SaveChangesAsync();
    }
}