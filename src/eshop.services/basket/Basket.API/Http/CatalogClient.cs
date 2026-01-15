namespace Basket.API.Http;

/// <summary>
/// Provides functionality to interact with the catalog service for product-related operations.
/// </summary>
public class CatalogClient(HttpClient client)
{
    /// <summary>
    /// Checks if a product with the specified identifier exists in the catalog.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to check.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the product exists; otherwise, false.</returns>
    public async Task<bool> ProductExists(Guid productId, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"{productId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
