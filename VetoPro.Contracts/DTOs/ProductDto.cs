using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs;

/// <summary>
/// DTO pour l'affichage d'un produit (article physique).
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Quantité actuellement en stock.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Prix de vente unitaire.
    /// </summary>
    [Required]
    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }
}