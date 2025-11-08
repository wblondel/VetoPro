using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Entities;
using VetoPro.Api.Helpers;
using VetoPro.Api.Mapping;
using VetoPro.Contracts.DTOs;
using VetoPro.Contracts.DTOs.Catalogs;
using FluentValidation;

namespace VetoPro.Api.Controllers.Catalogs;

[Authorize]
public class ProductsController(
    VetoProDbContext context,
    IValidator<ProductCreateDto> productCreateValidator,
    IValidator<ProductUpdateDto> productUpdateValidator)
    : BaseApiController(context)
{
    /// <summary>
    /// GET: api/products
    /// Récupère la liste de tous les produits (articles).
    /// </summary>
    /// <param name="paginationParams">Pagination parameters (pageNumber, pageSize).</param>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts([FromQuery] PaginationParams paginationParams)
    {
        var query = Context.Products
            .OrderBy(p => p.Name)
            .AsQueryable();
        
        return await CreatePaginatedResponse(query, paginationParams, p => p.ToDto());
    }

    /// <summary>
    /// GET: api/products/{id}
    /// Récupère un produit spécifique par son ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
    {
        var product = await Context.Products
            .Where(p => p.Id == id)
            .Select(p => p.ToDto())
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound("Produit non trouvé.");
        }

        return Ok(product);
    }

    /// <summary>
    /// POST: api/products
    /// Crée un nouveau produit (article).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductCreateDto createDto)
    {
        var validationResult = await productCreateValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Vérifier si le nom existe déjà (contrainte unique)
        if (await Context.Products.AnyAsync(p => p.Name == createDto.Name))
        {
            return Conflict("Un produit avec ce nom existe déjà.");
        }

        // Mapper le DTO vers l'Entité
        var newProduct = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            StockQuantity = createDto.StockQuantity,
            UnitPrice = createDto.UnitPrice,
            IsActive = createDto.IsActive
        };

        Context.Products.Add(newProduct);
        await Context.SaveChangesAsync();

        // Mapper l'entité créée vers le DTO de retour
        var productDto = newProduct.ToDto();

        return CreatedAtAction(nameof(GetProductById), new { id = productDto.Id }, productDto);
    }

    /// <summary>
    /// PUT: api/products/{id}
    /// Met à jour un produit existant.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin, Doctor")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDto updateDto)
    {
        var validationResult = await productUpdateValidator.ValidateAsync(updateDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        var productToUpdate = await Context.Products.FindAsync(id);

        if (productToUpdate == null)
        {
            return NotFound("Produit non trouvé.");
        }

        // Vérifier si le nouveau nom est déjà pris par un *autre* produit
        if (await Context.Products.AnyAsync(p => p.Name == updateDto.Name && p.Id != id))
        {
            return Conflict("Un autre produit avec ce nom existe déjà.");
        }

        // Appliquer les modifications
        productToUpdate.Name = updateDto.Name;
        productToUpdate.Description = updateDto.Description;
        productToUpdate.StockQuantity = updateDto.StockQuantity;
        productToUpdate.UnitPrice = updateDto.UnitPrice;
        productToUpdate.IsActive = updateDto.IsActive;

        await Context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }

    /// <summary>
    /// DELETE: api/products/{id}
    /// Supprime un produit.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var productToDelete = await Context.Products.FindAsync(id);

        if (productToDelete == null)
        {
            return NotFound("Produit non trouvé.");
        }

        // Sécurité : Vérifier si le produit est utilisé dans une InvoiceLine
        var isUsedInInvoice = await Context.InvoiceLines.AnyAsync(il => il.ProductId == id);
        if (isUsedInInvoice)
        {
            return BadRequest("Ce produit ne peut pas être supprimé car il est lié à une ou plusieurs factures.");
        }

        Context.Products.Remove(productToDelete);
        await Context.SaveChangesAsync();

        return NoContent();
    }
}