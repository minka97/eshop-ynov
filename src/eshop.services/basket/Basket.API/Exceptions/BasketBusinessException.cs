using BuildingBlocks.Exceptions;

namespace Basket.API.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a basket associated with a specific user
/// cannot be found in the system. This exception is typically used to indicate that the
/// requested basket resource does not exist or is inaccessible.
/// </summary>
public class BasketNotFoundException : NotFoundException
{
    /// <summary>
    /// Represents an exception that is thrown when a basket associated with a specific user
    /// cannot be found in the system. This exception is typically used to indicate that the
    /// requested basket resource does not exist or is inaccessible.
    /// </summary>
    public BasketNotFoundException(string userName) : base("panier", userName) { }
}

public class ProductNotFoundException : NotFoundException
{
    /// <summary>
    /// Represents an exception that is thrown when a specified product cannot be found.
    /// This exception is typically used to indicate that the requested product resource
    /// is missing or does not exist in the system.
    /// </summary>
    public ProductNotFoundException(string idProduit) : base("produit", idProduit) { }
}

