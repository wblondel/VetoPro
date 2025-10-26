using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Entities;
using UUIDNext;

namespace VetoPro.Api.Data;

/// <summary>
/// Le DbContext principal de l'application.
/// Il hérite de IdentityDbContext pour inclure les tables d'authentification (AspNetUsers, etc.).
/// </summary>
public class VetoProDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // --- Tables (DbSet) ---
    // Identity gère : Users (ApplicationUser), Roles, UserRoles, etc.
    
    // Entités "Personnes"
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<StaffDetails> StaffDetails { get; set; }
    
    // Entités "Animaux"
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Breed> Breeds { get; set; }
    public DbSet<Species> Species { get; set; }
    public DbSet<Color> Colors { get; set; }
    // La table de liaison 'patient_colors' est gérée par EF Core via la configuration M2M
    
    // Entités "Activité Clinique"
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    
    // Entités "Financier et Catalogue"
    public DbSet<Service> Services { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<PriceList> PriceList { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLine> InvoiceLines { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    
    public VetoProDbContext(DbContextOptions<VetoProDbContext> options) : base(options) { }
    
    /// <summary>
    /// Surcharge de SaveChanges pour injecter notre logique métier :
    /// 1. Génération d'UUIDv7 pour les nouvelles entités.
    /// 2. Horodatage automatique (CreatedAt / UpdatedAt).
    /// </summary>
    public override int SaveChanges()
    {
        ApplyCustomLogic();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyCustomLogic();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void ApplyCustomLogic()
    {
        // 1. Trouver toutes les entités qui héritent de BaseEntity
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added || 
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            var now = DateTime.UtcNow; // Toujours utiliser UTC

            // 2. Mettre à jour 'UpdatedAt' à chaque modification
            entity.UpdatedAt = now;

            // 3. Si l'entité est NOUVELLE (Added)
            if (entityEntry.State == EntityState.Added)
            {
                // Générer le nouvel UUIDv7
                entity.Id = Uuid.NewSequential();
                // Définir 'CreatedAt'
                entity.CreatedAt = now;
            }
        }
    }
    
    /// <summary>
    /// Configuration fine des relations et des contraintes (Fluent API).
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Indispensable : configure les tables Identity (AspNetUsers, etc.)
        base.OnModelCreating(modelBuilder);
        
        // --- Configuration des Contraintes Uniques ---
        
        // Un nom d'espèce doit être unique
        modelBuilder.Entity<Species>()
            .HasIndex(s => s.Name)
            .IsUnique();
        
        // Un nom de race doit être unique *par espèce*
        modelBuilder.Entity<Breed>()
            .HasIndex(b => new { b.SpeciesId, b.Name })
            .IsUnique();
        
        // Un nom de couleur doit être unique
        modelBuilder.Entity<Color>()
            .HasIndex(c => c.Name)
            .IsUnique();
        
        // Un numéro de puce doit être unique
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.ChipNumber)
            .IsUnique()
            .HasFilter("[ChipNumber] IS NOT NULL"); // Index unique seulement si non-null
        
        // Un UserId doit être unique dans Contacts (relation 1-à-1)
        modelBuilder.Entity<Contact>()
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");
        
        // Un ContactId doit être unique dans StaffDetails (relation 1-à-1)
        modelBuilder.Entity<StaffDetails>()
            .HasIndex(sd => sd.ContactId)
            .IsUnique();
        
        // Un AppointmentId doit être unique dans Consultations (relation 1-à-1)
        modelBuilder.Entity<Consultation>()
            .HasIndex(c => c.AppointmentId)
            .IsUnique();
        
        // Un nom de service doit être unique
        modelBuilder.Entity<Service>()
            .HasIndex(s => s.Name)
            .IsUnique();
                
        // Un nom de produit doit être unique
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .IsUnique();

        // Un numéro de facture doit être unique
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();
        
        // --- Configuration des Relations (comportements spécifiques) ---
        
        // Relation M2M Patient <-> Color
        modelBuilder.Entity<Patient>()
            .HasMany(p => p.Colors)
            .WithMany(c => c.Patients) // EF 9 est assez intelligent pour gérer la table de liaison
            .UsingEntity<Dictionary<string, object>>(
                "patient_colors", // Nom de votre table de liaison
                j => j.HasOne<Color>().WithMany().HasForeignKey("color_id"),
                j => j.HasOne<Patient>().WithMany().HasForeignKey("patient_id")
            );
        
        // Relations de Contact (pour éviter les suppressions en cascade)
        modelBuilder.Entity<Contact>()
            .HasMany(c => c.AppointmentsAsClient)
            .WithOne(a => a.Client)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict); // Ne pas supprimer un contact s'il a des RDV

        modelBuilder.Entity<Contact>()
            .HasMany(c => c.AppointmentsAsDoctor)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorContactId)
            .OnDelete(DeleteBehavior.SetNull); // Si on supprime le docteur, le RDV devient "non assigné"

        // Relations de Consultation (pour lever l'ambiguïté)
        modelBuilder.Entity<Consultation>(entity =>
        {
            // Relation avec le client
            entity.HasOne(c => c.Client)
                .WithMany(con => con.ConsultationsAsClient)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation avec le docteur
            entity.HasOne(c => c.Doctor)
                .WithMany(con => con.ConsultationsAsDoctor)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Un docteur ne peut être supprimé s'il a des consults
        });
        
        modelBuilder.Entity<Contact>()
            .HasMany(c => c.Invoices)
            .WithOne(i => i.Client)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict); // Ne pas supprimer un contact s'il a des factures

        modelBuilder.Entity<Contact>()
            .HasMany(c => c.PatientsOwned)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // Ne pas supprimer un contact s'il possède des animaux

        // Précision des types 'decimal' (SQLite ne le gère pas, mais c'est une bonne pratique)
        modelBuilder.Entity<Consultation>(e => {
            e.Property(c => c.WeightKg).HasColumnType("decimal(6, 2)");
            e.Property(c => c.TemperatureCelsius).HasColumnType("decimal(4, 1)");
        });

        modelBuilder.Entity<Product>(e => {
            e.Property(p => p.UnitPrice).HasColumnType("decimal(10, 2)");
        });
        
        modelBuilder.Entity<PriceList>(e => {
            e.Property(pl => pl.WeightMinKg).HasColumnType("decimal(6, 2)");
            e.Property(pl => pl.WeightMaxKg).HasColumnType("decimal(6, 2)");
            e.Property(pl => pl.Amount).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Invoice>(e => {
            e.Property(i => i.TotalAmount).HasColumnType("decimal(10, 2)");
        });
        
        modelBuilder.Entity<InvoiceLine>(e => {
            e.Property(il => il.Quantity).HasColumnType("decimal(10, 2)");
            e.Property(il => il.UnitPrice).HasColumnType("decimal(10, 2)");
            e.Property(il => il.LineTotal).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Payment>(e => {
            e.Property(p => p.Amount).HasColumnType("decimal(10, 2)");
        });
    }
}