namespace VetoPro.Api.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Entities;

public static class DataSeeder
{
    // C'est une méthode d'extension pour IHost, 
    // ce qui la rend facile à appeler depuis Program.cs
    public static async Task SeedDatabaseAsync(this IHost app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<VetoProDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                // 1. Appliquer les migrations (au cas où, bonne pratique)
                await context.Database.MigrateAsync();

                // 2. Seeding des Rôles (Admin, Doctor, Client)
                await SeedRolesAsync(roleManager);

                // 3. Seeding des Espèces
                await SeedSpeciesAsync(context);

                // 4. Seeding du Contact "Cabinet"
                await SeedClinicContactAsync(context);
            }
            catch (Exception ex)
            {
                // Gérer les erreurs de seeding (ex: logger)
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during database seeding.");
            }
        }
    }
    
    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("Doctor"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Doctor"));
        }
        if (!await roleManager.RoleExistsAsync("Client"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("Client"));
        }
    }
    
    private static async Task SeedSpeciesAsync(VetoProDbContext context)
    {
        // Si il y a déjà des espèces, on ne fait rien.
        if (await context.Species.AnyAsync())
        {
            return;
        }

        var dog = new Species { Name = "Chien" };
        var cat = new Species { Name = "Chat" };
        var horse = new Species { Name = "Cheval" };
        var bird = new Species { Name = "Oiseau" };
        var nac = new Species { Name = "NAC" }; // Nouveaux Animaux de Compagnie

        await context.Species.AddRangeAsync(dog, cat, horse, bird, nac);
        await context.SaveChangesAsync(); // SaveChangesAsync gérera les Id, CreatedAt, etc.
    }
    
    private static async Task SeedClinicContactAsync(VetoProDbContext context)
    {
        // On cherche un contact avec un nom spécifique pour éviter les doublons
        const string clinicName = "VetoPro";
        if (await context.Contacts.AnyAsync(c => c.LastName == clinicName))
        {
            return;
        }

        var clinicContact = new Contact
        {
            FirstName = "Cabinet",
            LastName = "VetoPro",
            Email = "contact@vetopro.com",
            PhoneNumber = "0102030405",
            IsOwner = true,  // Important: Ce contact peut "posséder" des animaux
            IsClient = true, // Important: Ce contact peut être facturé
            IsStaff = false
        };

        await context.Contacts.AddAsync(clinicContact);
        await context.SaveChangesAsync();
    }
}