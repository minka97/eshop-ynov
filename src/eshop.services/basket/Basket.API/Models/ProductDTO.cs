namespace Basket.API.Models;

/// <summary>
/// Represents a product within the catalog. Provides details such as product name, description,
/// price, associated categories, and an image file.
/// </summary>
public class ProductDTO
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }
}