using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Entities;

namespace VetoPro.Api.Data.SeedData;

public static class AuthSeeder
{
    public static async Task SeedInitialRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
        if (!await roleManager.RoleExistsAsync("Doctor"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("Doctor"));
        if (!await roleManager.RoleExistsAsync("Client"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("Client"));
    }

    public static async Task SeedClinicContact(VetoProDbContext context)
    {
        const string clinicName = "VetoPro";
        if (await context.Contacts.AnyAsync(c => c.LastName == clinicName))
            return;

        var clinicContact = new Contact
        {
            FirstName = "Cabinet",
            LastName = clinicName,
            Email = "contact@vetopro.com",
            IsOwner = true, // Important: Ce contact peut "posséder" des animaux
            IsClient = true, // Important: Ce contact peut être facturé
            IsStaff = false
        };
        await context.Contacts.AddAsync(clinicContact);
        await context.SaveChangesAsync();
    }

    public static async Task SeedInitialStaff(VetoProDbContext context, UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@vetopro.com";
        const string adminPassword = "AdminPassword1!"; // CHANGE THIS PASSWORD!

        if (await userManager.FindByEmailAsync(adminEmail) != null)
            return;

        var adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var identityResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (!identityResult.Succeeded)
            return; // Exit if user creation failed

        await userManager.AddToRoleAsync(adminUser, "Admin");

        var adminContact = new Contact
        {
            FirstName = "Alice",
            LastName = "Smith",
            Email = adminEmail,
            PhoneNumber = "+33123456789",
            AddressLine1 = "1 Rue de l'Administration",
            City = "VetoCity",
            PostalCode = "78000",
            Country = "France",
            IsOwner = false,
            IsClient = true,
            IsStaff = true,
            UserId = adminUser.Id
        };
        context.Contacts.Add(adminContact);
        await context.SaveChangesAsync();

        var adminStaffDetails = new StaffDetails
        {
            ContactId = adminContact.Id,
            Role = "Administrator",
            LicenseNumber = "ADMIN001",
            Specialty = "System Management",
            IsActive = true
        };
        context.StaffDetails.Add(adminStaffDetails);
        await context.SaveChangesAsync();
    }
}