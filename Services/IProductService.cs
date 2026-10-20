// ==============================================================================
// IProductService.cs
// Namespace: OnlineShoppingSystem.Services
// Description: Contract defining asynchronous product operations, including
// API retrieval, memory caching (10 min expiry), and category filtering.
// ==============================================================================

using OnlineShoppingSystem.Models;

namespace OnlineShoppingSystem.Services
{
    /// <summary>
    /// Service interface defining operations for managing and retrieving products.
    /// Follows Dependency Inversion Principle (DIP) to allow loose coupling.
    /// </summary>
    public interface IProductService
    {
        // --------------------------------------------------------------------------
        // Requirement 2 & 4: GetProductsAsync
        // Asynchronously retrieves all products, cached with IMemoryCache (10-min expiration).
        // Parameter forceRefresh: When true, bypasses the cache and queries FakeStoreAPI anew.
        // Returns: Tuple containing product list, cache status indicator, and timestamp.
        // --------------------------------------------------------------------------
        Task<(IEnumerable<Product> Products, string CacheStatus, DateTime CacheTimestamp)> GetProductsAsync(bool forceRefresh = false);

        // --------------------------------------------------------------------------
        // GetProductByIdAsync
        // Asynchronously retrieves a single product by its unique ProductId.
        // Used by Requirement 3 (Query String) and Requirement 4 (Response Caching).
        // --------------------------------------------------------------------------
        Task<Product?> GetProductByIdAsync(int productId);

        // --------------------------------------------------------------------------
        // GetCategoriesAsync
        // Asynchronously retrieves all distinct product categories.
        // Used by Requirement 3 (Session State category selection).
        // --------------------------------------------------------------------------
        Task<IEnumerable<string>> GetCategoriesAsync();

        // --------------------------------------------------------------------------
        // ClearProductCache
        // Evicts the product list cache entry from IMemoryCache manually.
        // --------------------------------------------------------------------------
        void ClearProductCache();
    }
}
