using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

                // 2. Seeding des Rôles (Admin, Doctor, Client)
                await SeedRolesAsync(roleManager);

                // 3. Seeding des Catalogs
                await SeedSpeciesAsync(context);
                await SeedColorsAsync(context);
                await SeedBreedsAsync(context);
                await SeedServicesAsync(context);
                await SeedProductsAsync(context);
                
                // 4. Seeding du Contact "Cabinet"
                await SeedClinicContactAsync(context);
                await SeedInitialStaffAsync(context, userManager);
            }
            catch (Exception ex)
            {
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
    
    private static async Task SeedColorsAsync(VetoProDbContext context)
    {
        // If colors already exist, do nothing.
        if (await context.Colors.AnyAsync())
        {
            return;
        }

        var colors = new List<Color>
        {
            new() { Name = "Noir", HexValue = "#000000" },
            new() { Name = "Blanc", HexValue = "#FFFFFF" },
            new() { Name = "Gris", HexValue = "#808080" },
            new() { Name = "Roux", HexValue = "#D2691E" }, // Chocolat color
            new() { Name = "Beige", HexValue = "#F5F5DC" },
            new() { Name = "Tigré" }, // Brindle/Tabby
            new() { Name = "Tricolore" }, // Calico/Tortoiseshell
            new() { Name = "Bleu" }, // Often used for grey cats/dogs
            new() { Name = "Fauve" } // Fawn color
        };

        await context.Colors.AddRangeAsync(colors);
        await context.SaveChangesAsync(); // SaveChangesAsync handles Id, CreatedAt, etc.
    }
    
    private static async Task SeedBreedsAsync(VetoProDbContext context)
    {
        // If breeds already exist, do nothing.
        if (await context.Breeds.AnyAsync())
        {
            return;
        }

        // Get the IDs of the species we need
        var dogSpecies = await context.Species.FirstOrDefaultAsync(s => s.Name == "Chien");
        var catSpecies = await context.Species.FirstOrDefaultAsync(s => s.Name == "Chat");

        if (dogSpecies == null || catSpecies == null)
        {
            // Log an error or throw an exception if species seeding failed
            Console.WriteLine("ERROR: Cannot seed breeds because required species (Chien, Chat) were not found.");
            return;
        }

        var breeds = new List<Breed>
        {
            // Dog Breeds
            new() { SpeciesId = dogSpecies.Id, Name = "Labrador Retriever" },
            new() { SpeciesId = dogSpecies.Id, Name = "Berger Allemand" },
            new() { SpeciesId = dogSpecies.Id, Name = "Golden Retriever" },
            new() { SpeciesId = dogSpecies.Id, Name = "Bouledogue Français" },
            new() { SpeciesId = dogSpecies.Id, Name = "Caniche" },
            new() { SpeciesId = dogSpecies.Id, Name = "Européen" }, // Common mix breed category

            // Cat Breeds
            new() { SpeciesId = catSpecies.Id, Name = "Européen" }, // Common mix breed category
            new() { SpeciesId = catSpecies.Id, Name = "Maine Coon" },
            new() { SpeciesId = catSpecies.Id, Name = "Persan" },
            new() { SpeciesId = catSpecies.Id, Name = "Siamois" },
            new() { SpeciesId = catSpecies.Id, Name = "Chartreux" }
        };

        await context.Breeds.AddRangeAsync(breeds);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedServicesAsync(VetoProDbContext context)
    {
        // If services already exist, do nothing.
        if (await context.Services.AnyAsync())
        {
            return;
        }

        var services = new List<Service>
        {
            // Consultations
            new() { Name = "Consultation générale", RequiresPatient = true },
            new() { Name = "Consultation de suivi", RequiresPatient = true },
            new() { Name = "Consultation d'urgence", RequiresPatient = true },
            new() { Name = "Consultation comportementale", RequiresPatient = true },

            // Vaccinations (examples, often species-specific pricing)
            new() { Name = "Vaccination CHPPiL (Chien)", RequiresPatient = true },
            new() { Name = "Vaccination TCL (Chat)", RequiresPatient = true },
            new() { Name = "Vaccination Rage", RequiresPatient = true },

            // Chirurgie
            new() { Name = "Stérilisation (Chatte)", RequiresPatient = true },
            new() { Name = "Castration (Chat)", RequiresPatient = true },
            new() { Name = "Stérilisation (Chienne)", RequiresPatient = true },
            new() { Name = "Castration (Chien)", RequiresPatient = true },
            new() { Name = "Détartrage", RequiresPatient = true },

            // Autres Actes
            new() { Name = "Identification Puce Electronique", RequiresPatient = true },
            new() { Name = "Analyse sanguine", RequiresPatient = true },
            new() { Name = "Radiographie", RequiresPatient = true },
            new() { Name = "Euthanasie", RequiresPatient = true },

            // Vente (requires_patient = false)
            new() { Name = "Frais de dossier", RequiresPatient = false },
        };

        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedProductsAsync(VetoProDbContext context)
    {
        // If products already exist, do nothing.
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var products = new List<Product>
        {
            // Food (examples)
            new() { Name = "Croquettes Chien Adulte Standard 3kg", UnitPrice = 25.50m, StockQuantity = 50 },
            new() { Name = "Croquettes Chien Stérilisé Light 10kg", UnitPrice = 65.00m, StockQuantity = 20 },
            new() { Name = "Croquettes Chat Adulte Saumon 1.5kg", UnitPrice = 18.90m, StockQuantity = 40 },
            new() { Name = "Croquettes Chat Stérilisé Urinaire 7kg", UnitPrice = 55.75m, StockQuantity = 15 },
            new() { Name = "Pâtée Chat Senior Poulet 85g", UnitPrice = 1.20m, StockQuantity = 100 },

            // Medications / Parasite Control (examples)
            new() { Name = "Antiparasitaire Pipette Chat <4kg", UnitPrice = 8.50m, StockQuantity = 80 },
            new() { Name = "Antiparasitaire Pipette Chien 10-20kg", UnitPrice = 12.00m, StockQuantity = 60 },
            new() { Name = "Vermifuge Comprimé Chien/Chat", UnitPrice = 5.00m, StockQuantity = 120 },

            // Accessories (examples)
            new() { Name = "Shampooing Doux Universel 250ml", UnitPrice = 9.90m, StockQuantity = 30 },
            new() { Name = "Collier Anti-Puces Chien Moyen", UnitPrice = 22.00m, StockQuantity = 25 }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedInitialStaffAsync(
        VetoProDbContext context,
        UserManager<ApplicationUser> userManager) 
    {
        const string adminEmail = "admin@vetopro.com";
        const string adminPassword = "AdminPassword1!";
        const string adminRole = "Admin";
        const string adminFirstName = "Alice";
        const string adminLastName = "Smith";

        // 1. Check if the user already exists
        if (await userManager.FindByEmailAsync(adminEmail) != null)
        {
            return;
        }

        // 2. Create the ApplicationUser (Identity)
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var identityResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (!identityResult.Succeeded)
        {
            // Log this failure in a real application
            Console.WriteLine($"ERROR: Failed to create Admin User: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
            return;
        }

        // 3. Assign the "Admin" role
        if (await userManager.IsInRoleAsync(adminUser, adminRole) == false)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }

        // 4. Create the Contact Profile
        var adminContact = new Contact
        {
            FirstName = adminFirstName,
            LastName = adminLastName,
            Email = adminEmail,
            PhoneNumber = "+33123456789",
            AddressLine1 = "1 Rue de l'Administration",
            City = "VetoCity",
            PostalCode = "78000",
            Country = "France",
            IsOwner = false,
            IsClient = true,
            IsStaff = true, // Crucial
            UserId = adminUser.Id // Link to the Identity user
        };

        context.Contacts.Add(adminContact);
        await context.SaveChangesAsync(); // Save the contact to get its ID

        // 5. Create the StaffDetails (linking the Contact profile)
        var adminStaffDetails = new StaffDetails
        {
            ContactId = adminContact.Id,
            Role = "Administrator",
            LicenseNumber = "ADMIN001", // Placeholder
            Specialty = "System Management",
            IsActive = true
        };

        context.StaffDetails.Add(adminStaffDetails);
        await context.SaveChangesAsync();
    }
}