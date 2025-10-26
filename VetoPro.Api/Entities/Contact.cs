using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetoPro.Api.Entities;

/// <summary>
/// Entité centrale représentant une personne physique.
/// Elle peut être un propriétaire (Owner), un client (Client) ou un membre du personnel (Staff).
/// </summary>
public class Contact : BaseEntity
{
    /// <summary>
    /// Clé étrangère (optionnelle) vers l'entité d'authentification AspNetUsers.
    /// Non-nulle si ce contact a un compte pour se connecter.
    /// C'est le point de liaison unique entre le profil métier et le compte utilisateur.
    /// </summary>
    [ForeignKey("User")]
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; } // Propriété de navigation 1-à-1 (optionnelle)
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(255)]
    public string? AddressLine1 { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Ce contact est-il propriétaire d'un (ou plusieurs) animal ?
    /// </summary>
    public bool IsOwner { get; set; }
    
    /// <summary>
    /// Ce contact est-il un client (prend des RDV, paie des factures) ?
    /// </summary>
    public bool IsClient { get; set; }
    
    /// <summary>
    /// Ce contact fait-il partie du personnel ?
    /// </summary>
    public bool IsStaff { get; set; }
    
    // --- Propriétés de Navigation ---
    
    /// <summary>
    /// Relation 1-à-1 vers les détails spécifiques du staff (si IsStaff = true).
    /// </summary>
    public StaffDetails? StaffDetails { get; set; }
    
    /// <summary>
    /// Liste des patients dont ce contact est le propriétaire.
    /// </summary>
    public ICollection<Patient> PatientsOwned { get; set; } = new List<Patient>();
    
    /// <summary>
    /// Liste des RDV pris par ce contact en tant que client.
    /// </summary>
    public ICollection<Appointment> AppointmentsAsClient { get; set; } = new List<Appointment>();
    
    /// <summary>
    /// Liste des RDV assignés à ce contact en tant que docteur.
    /// </summary>
    public ICollection<Appointment> AppointmentsAsDoctor { get; set; } = new List<Appointment>();
    
    /// <summary>
    /// Liste des consultations où ce contact était le client présent.
    /// </summary>
    public ICollection<Consultation> ConsultationsAsClient { get; set; } = new List<Consultation>();
    
    /// <summary>
    /// Liste des consultations effectuées par ce contact en tant que docteur.
    /// </summary>
    public ICollection<Consultation> ConsultationsAsDoctor { get; set; } = new List<Consultation>();
    
    /// <summary>
    /// Liste des factures adressées à ce contact.
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}