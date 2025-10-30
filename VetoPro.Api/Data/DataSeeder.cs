using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data.SeedData;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Data;

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
                
                // 2. Authentification & roles (ordre strict)
                await AuthSeeder.SeedInitialRoles(roleManager);
                await AuthSeeder.SeedClinicContact(context);
                await AuthSeeder.SeedInitialStaff(context, userManager);
                
                // 3. CATALOGUES STATIQUES (Ordre par Dépendance)
                await CatalogSeeder.SeedSpecies(context);
                await CatalogSeeder.SeedColors(context);
                await CatalogSeeder.SeedBreeds(context);
                await CatalogSeeder.SeedServices(context);
                await CatalogSeeder.SeedProducts(context);
                await CatalogSeeder.SeedPriceLists(context);
                
                // 4. DONNÉES EN VITALITÉ (Ordre par Dépendance)
                await ActivitySeeder.SeedInitialPatients(context);
                await ActivitySeeder.SeedAppointments(context);
                await ActivitySeeder.SeedConsultations(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during database seeding.");
                // Optionnel: lancer une exception pour arrêter le démarrage si le seeding est critique
            }
        }
    }
}