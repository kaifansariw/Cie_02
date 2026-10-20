// ==============================================================================
// FakeStoreProductDto.cs
// Namespace: OnlineShoppingSystem.Models
// Description: Data Transfer Object (DTO) matching the JSON schema from FakeStoreAPI.
// Used for Requirement 2: Asynchronous Web Request to https://fakestoreapi.com/products
// ==============================================================================

using System.Text.Json.Serialization;

namespace OnlineShoppingSystem.Models
{
    /// <summary>
    /// Data Transfer Object representing the raw JSON structure returned by FakeStoreAPI.
    /// This object isolates external API structure from our internal domain Product model.
    /// </summary>
    public class FakeStoreProductDto
    {
        // --------------------------------------------------------------------------
        // Id: Primary key from FakeStoreAPI
        // --------------------------------------------------------------------------
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // --------------------------------------------------------------------------
        // Title: Product name/headline from FakeStoreAPI
        // --------------------------------------------------------------------------
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Price: Decimal price value from FakeStoreAPI
        // --------------------------------------------------------------------------
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        // --------------------------------------------------------------------------
        // Description: Long text overview from FakeStoreAPI
        // --------------------------------------------------------------------------
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Category: Product classification from FakeStoreAPI
        // --------------------------------------------------------------------------
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Image: URL string pointing to the product photo
        // --------------------------------------------------------------------------
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Rating: Nested rating object containing rate and count
        // --------------------------------------------------------------------------
        [JsonPropertyName("rating")]
        public FakeStoreRatingDto? Rating { get; set; }
    }

    /// <summary>
    /// Nested rating structure inside FakeStoreProductDto.
    /// </summary>
    public class FakeStoreRatingDto
    {
        // --------------------------------------------------------------------------
        // Rate: Average star rating (e.g., 3.9)
        // --------------------------------------------------------------------------
        [JsonPropertyName("rate")]
        public double Rate { get; set; }

        // --------------------------------------------------------------------------
        // Count: Total number of customer reviews
        // --------------------------------------------------------------------------
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
