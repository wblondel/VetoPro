using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour la création d'un nouveau produit (article physique).
/// </summary>
public class ProductCreateDto
{
    [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Le stock ne peut pas être négatif.")]
    public int StockQuantity { get; set; } = 0;

    [Required(ErrorMessage = "Le prix unitaire est obligatoire.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Le prix ne peut pas être négatif.")]
    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;
}