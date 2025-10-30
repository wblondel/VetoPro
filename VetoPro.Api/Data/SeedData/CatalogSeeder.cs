using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Data.SeedData;

public static class CatalogSeeder
{
    public static async Task SeedSpecies(VetoProDbContext context)
    {
        if (await context.Species.AnyAsync()) return;
        
        var species = new List<Species>
        {
            new() { Name = "Chien" },
            new() { Name = "Chat" },
            new() { Name = "Cheval" },
            new() { Name = "Oiseau" },
            new() { Name = "Hamster"}
        };

        await context.Species.AddRangeAsync(species);
        await context.SaveChangesAsync();
    }

    public static async Task SeedColors(VetoProDbContext context)
    {
        if (await context.Colors.AnyAsync()) return;
        
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
        await context.SaveChangesAsync();
    }

    public static async Task SeedBreeds(VetoProDbContext context)
    {
        if (await context.Breeds.AnyAsync()) return;
        
        var dogSpecies = await context.Species.FirstOrDefaultAsync(s => s.Name == "Chien");
        var catSpecies = await context.Species.FirstOrDefaultAsync(s => s.Name == "Chat");
        
        if (dogSpecies == null || catSpecies == null) return;

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

    public static async Task SeedServices(VetoProDbContext context)
    {
        if (await context.Services.AnyAsync()) return;
        
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
    
    public static async Task SeedProducts(VetoProDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

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
    
    public static async Task SeedPriceLists(VetoProDbContext context)
    {
        if (await context.PriceList.AnyAsync()) return;

        // --- Retrieve necessary IDs ---
        var consultService = await context.Services.FirstOrDefaultAsync(s => s.Name == "Consultation générale");
        var vaccineDogService = await context.Services.FirstOrDefaultAsync(s => s.Name == "Vaccination CHPPiL (Chien)");
        var vaccineCatService = await context.Services.FirstOrDefaultAsync(s => s.Name == "Vaccination TCL (Chat)");
        var spayDogService = await context.Services.FirstOrDefaultAsync(s => s.Name == "Stérilisation (Chienne)");
        var spayCatService = await context.Services.FirstOrDefaultAsync(s => s.Name == "Stérilisation (Chatte)");
        var castrateDogService = await context.Services.FirstOrDefaultAsync(s => s.Name == "Castration (Chien)");

        var dogSpecies = await context.Species.FirstOrDefaultAsync(s => s.Name == "Chien");
        var catSpecies = await context.Species.FirstOrDefaultAsync(s => s.Name == "Chat");

        // Check for critical dependencies
        if (consultService == null || dogSpecies == null || catSpecies == null || spayDogService == null ||
            spayCatService == null)
        {
            Console.WriteLine("ERROR: Cannot seed price list due to missing service or species dependencies.");
            return;
        }

        var priceRules = new List<PriceList>();
        const string currency = "EUR";

        // 1. Consultation Générale (Applies to all species, fixed price)
        priceRules.Add(new PriceList
        {
            ServiceId = consultService.Id,
            Amount = 45.00m,
            Currency = currency,
            IsActive = true
        });

        // 2. Stérilisation Chienne (Price depends on weight/species)
        // Small Dog (< 10kg)
        priceRules.Add(new PriceList
        {
            ServiceId = spayDogService.Id,
            SpeciesId = dogSpecies.Id,
            WeightMaxKg = 10.0m,
            Amount = 180.00m,
            Currency = currency,
            IsActive = true
        });
        // Medium Dog (10.1kg - 25kg)
        priceRules.Add(new PriceList
        {
            ServiceId = spayDogService.Id,
            SpeciesId = dogSpecies.Id,
            WeightMinKg = 10.1m,
            WeightMaxKg = 25.0m,
            Amount = 240.00m,
            Currency = currency,
            IsActive = true
        });
        // Large Dog (> 25kg)
        priceRules.Add(new PriceList
        {
            ServiceId = spayDogService.Id,
            SpeciesId = dogSpecies.Id,
            WeightMinKg = 25.1m,
            Amount = 300.00m,
            Currency = currency,
            IsActive = true
        });

        // 3. Stérilisation Chatte (Fixed price for cats)
        priceRules.Add(new PriceList
        {
            ServiceId = spayCatService.Id,
            SpeciesId = catSpecies.Id,
            Amount = 120.00m,
            Currency = currency,
            IsActive = true
        });

        // 4. Castration Chien (Fixed price example)
        if (castrateDogService != null)
        {
            priceRules.Add(new PriceList
            {
                ServiceId = castrateDogService.Id,
                SpeciesId = dogSpecies.Id,
                Amount = 150.00m,
                Currency = currency,
                IsActive = true
            });
        }

        // 5. Vaccinations (Fixed price)
        if (vaccineDogService != null)
        {
            priceRules.Add(new PriceList
            {
                ServiceId = vaccineDogService.Id,
                SpeciesId = dogSpecies.Id,
                Amount = 55.00m,
                Currency = currency,
                IsActive = true
            });
        }

        if (vaccineCatService != null)
        {
            priceRules.Add(new PriceList
            {
                ServiceId = vaccineCatService.Id,
                SpeciesId = catSpecies.Id,
                Amount = 50.00m,
                Currency = currency,
                IsActive = true
            });
        }

        await context.PriceList.AddRangeAsync(priceRules);
        await context.SaveChangesAsync();
    }
}