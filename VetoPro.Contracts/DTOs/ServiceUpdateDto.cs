using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la mise à jour (PUT) d'un service ou acte existant.
/// </summary>
public class ServiceUpdateDto
{
    [Required(ErrorMessage = "Le nom du service est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indique si ce service nécessite un patient (true)
    /// ou peut être vendu "au comptoir" (false).
    /// </summary>
    public bool RequiresPatient { get; set; } = true;
}