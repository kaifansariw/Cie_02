// ==============================================================================
// ProductCatalogViewModel.cs
// Namespace: OnlineShoppingSystem.Models
// Description: ViewModel consolidating all data required for the product catalog view.
// Combines products, category session filter, customer cookie, and cache metadata.
// ==============================================================================

using System.Collections.Generic;

namespace OnlineShoppingSystem.Models
{
    /// <summary>
    /// ViewModel passed to the Product Catalog (Index) view.
    /// Provides strongly-typed data binding for product listings, filter states,
    /// customer identification from cookies, and cache diagnostics.
    /// </summary>
    public class ProductCatalogViewModel
    {
        // --------------------------------------------------------------------------
        // Products: The collection of products to display in table/grid views
        // --------------------------------------------------------------------------
        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        // --------------------------------------------------------------------------
        // Categories: Distinct list of all available categories for the filter dropdown
        // --------------------------------------------------------------------------
        public IEnumerable<string> Categories { get; set; } = new List<string>();

        // --------------------------------------------------------------------------
        // Requirement 3: SelectedCategory (State Management via Session)
        // Description: The currently active category filter retrieved from Session state.
        // --------------------------------------------------------------------------
        public string? SelectedCategory { get; set; }

        // --------------------------------------------------------------------------
        // Requirement 3: CustomerName (State Management via Cookie)
        // Description: The customer's personalized name retrieved from HTTP Cookies.
        // --------------------------------------------------------------------------
        public string? CustomerName { get; set; }

        // --------------------------------------------------------------------------
        // Requirement 4: CacheStatus (Caching Diagnostics)
        // Description: Displays whether the data was served from IMemoryCache or FakeStoreAPI.
        // --------------------------------------------------------------------------
        public string CacheStatus { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // CacheTimestamp: The exact time the cached item was created or retrieved.
        // --------------------------------------------------------------------------
        public DateTime? CacheTimestamp { get; set; }

        // --------------------------------------------------------------------------
        // SearchTerm: Optional keyword search filter across product names and descriptions.
        // --------------------------------------------------------------------------
        public string? SearchTerm { get; set; }

        // --------------------------------------------------------------------------
        // TotalProductCount: Total count of products in the current filtered view.
        // --------------------------------------------------------------------------
        public int TotalProductCount { get; set; }
    }
}
