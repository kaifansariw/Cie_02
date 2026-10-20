// ==============================================================================
// ProductService.cs
// Namespace: OnlineShoppingSystem.Services
// Description: Concrete service implementation for fetching product data.
// Handles:
// - Requirement 2: Asynchronous web requests to https://fakestoreapi.com/products
// - Requirement 4: In-memory caching with IMemoryCache (10-minute expiration)
// ==============================================================================

using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using OnlineShoppingSystem.Models;

namespace OnlineShoppingSystem.Services
{
    /// <summary>
    /// Implements product data retrieval with asynchronous web calls to FakeStoreAPI
    /// and performance optimization using ASP.NET Core IMemoryCache.
    /// </summary>
    public class ProductService : IProductService
    {
        // --------------------------------------------------------------------------
        // Private Read-only Dependencies
        // Injected via Dependency Injection (DI) through the constructor.
        // --------------------------------------------------------------------------
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ProductService> _logger;

        // --------------------------------------------------------------------------
        // Cache Configuration Constants
        // --------------------------------------------------------------------------
        // Cache key identifier for storing the full product collection
        private const string ProductsCacheKey = "ONLINE_SHOPPING_PRODUCTS_LIST";
        // Cache key for storing the timestamp when the products were cached
        private const string ProductsCacheTimestampKey = "ONLINE_SHOPPING_PRODUCTS_TIMESTAMP";
        // FakeStoreAPI endpoint URL as specified in Requirement 2
        private const string FakeStoreApiUrl = "https://fakestoreapi.com/products";
        // Requirement 4: Cache duration set to 10 minutes
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Constructor: Injects HttpClient, IMemoryCache, and ILogger via DI.
        /// </summary>
        /// <param name="httpClient">Injected HTTP client for async API requests</param>
        /// <param name="memoryCache">Injected memory cache for caching products</param>
        /// <param name="logger">Logger instance for operational logging</param>
        public ProductService(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            ILogger<ProductService> logger)
        {
            // Assign injected HTTP client to private field
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Assign injected memory cache to private field
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            // Assign injected logger to private field
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --------------------------------------------------------------------------
        // Requirement 2: Asynchronous Web Request
        // Requirement 4: Caching with IMemoryCache (10-minute expiration)
        // --------------------------------------------------------------------------
        public async Task<(IEnumerable<Product> Products, string CacheStatus, DateTime CacheTimestamp)> GetProductsAsync(bool forceRefresh = false)
        {
            // Check if a force-refresh was requested by the user
            if (forceRefresh)
            {
                // Evict existing cache entries
                _memoryCache.Remove(ProductsCacheKey);
                _memoryCache.Remove(ProductsCacheTimestampKey);
                _logger.LogInformation("Product cache manually invalidated by user request.");
            }

            // Step 1: Attempt to retrieve products from IMemoryCache first
            if (!forceRefresh && _memoryCache.TryGetValue(ProductsCacheKey, out List<Product>? cachedProducts) && cachedProducts != null)
            {
                // Retrieve the timestamp when this cache entry was created
                _memoryCache.TryGetValue(ProductsCacheTimestampKey, out DateTime cachedTime);

                _logger.LogInformation("Serving {Count} products directly from IMemoryCache (10-min expiration).", cachedProducts.Count);

                // Return cached data with diagnostic message
                return (cachedProducts, "Served from IMemoryCache (10-minute sliding/absolute expiration)", cachedTime);
            }

            // Step 2: Cache miss or force refresh - fetch asynchronously from external API
            _logger.LogInformation("Cache miss or refresh. Fetching products asynchronously from {Url}...", FakeStoreApiUrl);

            List<Product> fetchedProducts;
            DateTime fetchTime = DateTime.UtcNow;

            try
            {
                // Perform asynchronous HTTP GET request to FakeStoreAPI
                // Using ReadFromJsonAsync to stream and deserialize JSON response
                var dtos = await _httpClient.GetFromJsonAsync<List<FakeStoreProductDto>>(FakeStoreApiUrl);

                if (dtos != null && dtos.Count > 0)
                {
                    // Map external FakeStoreProductDto objects to our domain Product model
                    fetchedProducts = dtos.Select(dto => MapDtoToProduct(dto)).ToList();
                    _logger.LogInformation("Successfully fetched and mapped {Count} products from FakeStoreAPI.", fetchedProducts.Count);
                }
                else
                {
                    // Fallback to local default dataset if API returned empty
                    _logger.LogWarning("FakeStoreAPI returned empty response. Loading built-in seed products.");
                    fetchedProducts = GetFallbackProducts();
                }
            }
            catch (Exception ex)
            {
                // Graceful error recovery: If FakeStoreAPI is unreachable, log warning and use fallback catalog
                _logger.LogError(ex, "Failed to asynchronously fetch products from FakeStoreAPI. Utilizing fallback catalog.");
                fetchedProducts = GetFallbackProducts();
            }

            // Step 3: Requirement 4 - Store the fetched product list in IMemoryCache with 10-minute expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Set absolute expiration to exactly 10 minutes from now as specified in Requirement 4
                .SetAbsoluteExpiration(CacheDuration)
                // Set priority to High so memory pressure won't evict it prematurely
                .SetPriority(CacheItemPriority.High);

            // Save product collection into cache
            _memoryCache.Set(ProductsCacheKey, fetchedProducts, cacheEntryOptions);
            // Save cache timestamp
            _memoryCache.Set(ProductsCacheTimestampKey, fetchTime, cacheEntryOptions);

            _logger.LogInformation("Stored {Count} products in IMemoryCache with 10-minute expiration.", fetchedProducts.Count);

            // Return fresh data with diagnostic message
            return (fetchedProducts, "Freshly fetched via Asynchronous Web Request (FakeStoreAPI)", fetchTime);
        }

        // --------------------------------------------------------------------------
        // GetProductByIdAsync
        // Retrieves a single product by ID, utilizing the cached product collection
        // --------------------------------------------------------------------------
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            // Retrieve all products (which will hit memory cache if populated)
            var (products, _, _) = await GetProductsAsync(forceRefresh: false);

            // Find product matching the requested ProductId
            var product = products.FirstOrDefault(p => p.ProductId == productId);

            return product;
        }

        // --------------------------------------------------------------------------
        // GetCategoriesAsync
        // Returns distinct categories from the product collection for session filtering
        // --------------------------------------------------------------------------
        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            // Retrieve all products
            var (products, _, _) = await GetProductsAsync(forceRefresh: false);

            // Extract distinct, non-empty categories sorted alphabetically
            var categories = products
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            return categories;
        }

        // --------------------------------------------------------------------------
        // ClearProductCache
        // Manually removes the product entries from IMemoryCache
        // --------------------------------------------------------------------------
        public void ClearProductCache()
        {
            _memoryCache.Remove(ProductsCacheKey);
            _memoryCache.Remove(ProductsCacheTimestampKey);
            _logger.LogInformation("Product cache explicitly evicted via ClearProductCache().");
        }

        // --------------------------------------------------------------------------
        // Helper Method: MapDtoToProduct
        // Maps FakeStoreProductDto to domain Product, assigning realistic discount percentage
        // --------------------------------------------------------------------------
        private static Product MapDtoToProduct(FakeStoreProductDto dto)
        {
            // Deterministic discount percentage assignment based on product ID
            // Creates diverse discounts across items: 10%, 15%, 20%, 25%, 30%, 0%
            decimal[] discountTiers = [10.0m, 15.0m, 20.0m, 25.0m, 30.0m, 0.0m, 18.0m, 12.0m];
            decimal assignedDiscount = discountTiers[Math.Abs(dto.Id) % discountTiers.Length];

            return new Product
            {
                ProductId = dto.Id,
                ProductName = dto.Title,
                Category = dto.Category,
                Price = dto.Price,
                DiscountPercentage = assignedDiscount,
                Description = dto.Description,
                Image = dto.Image,
                RatingRate = dto.Rating?.Rate ?? 4.0,
                RatingCount = dto.Rating?.Count ?? 50
            };
        }

        // --------------------------------------------------------------------------
        // Helper Method: GetFallbackProducts
        // Supplies realistic sample products if external network is unavailable
        // --------------------------------------------------------------------------
        private static List<Product> GetFallbackProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "Fjallraven - Foldsack No. 1 Backpack, Fits 15 Laptops",
                    Category = "men's clothing",
                    Price = 109.95m,
                    DiscountPercentage = 15.0m,
                    Description = "Your perfect pack for everyday use and walks in the forest. Stash your laptop in the padded sleeve.",
                    Image = "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
                    RatingRate = 3.9,
                    RatingCount = 120
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Mens Casual Premium Slim Fit T-Shirts",
                    Category = "men's clothing",
                    Price = 22.30m,
                    DiscountPercentage = 10.0m,
                    Description = "Slim-fitting style, contrast raglan long sleeve, three-button henley placket, light weight & soft fabric for breathable and comfortable wearing.",
                    Image = "https://fakestoreapi.com/img/71-3HjGNDUL._AC_SY879._SX._UX._SY._UY_.jpg",
                    RatingRate = 4.1,
                    RatingCount = 259
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Mens Cotton Jacket",
                    Category = "men's clothing",
                    Price = 55.99m,
                    DiscountPercentage = 20.0m,
                    Description = "Great outerwear jackets for Spring/Autumn/Winter, suitable for many occasions, such as working, hiking, camping, mountain/rock climbing, cycling.",
                    Image = "https://fakestoreapi.com/img/71li-ujtlUL._AC_UX679_.jpg",
                    RatingRate = 4.7,
                    RatingCount = 500
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Mens Casual Slim Fit",
                    Category = "men's clothing",
                    Price = 15.99m,
                    DiscountPercentage = 25.0m,
                    Description = "The color could be slightly different between on the screen and in practice. Please note that body builds vary by person.",
                    Image = "https://fakestoreapi.com/img/71YXzeOuslL._AC_UY879_.jpg",
                    RatingRate = 2.1,
                    RatingCount = 430
                },
                new Product
                {
                    ProductId = 5,
                    ProductName = "John Hardy Women's Legends Naga Gold & Silver Dragon Station Chain Bracelet",
                    Category = "jewelery",
                    Price = 695.00m,
                    DiscountPercentage = 10.0m,
                    Description = "From our Legends Collection, the Naga was inspired by the mythical water dragon that protects the ocean's pearl.",
                    Image = "https://fakestoreapi.com/img/71pWzhdJNwL._AC_UL640_QL65_ML3_.jpg",
                    RatingRate = 4.6,
                    RatingCount = 400
                },
                new Product
                {
                    ProductId = 6,
                    ProductName = "Solid Gold Petite Micropave",
                    Category = "jewelery",
                    Price = 168.00m,
                    DiscountPercentage = 30.0m,
                    Description = "Satisfaction Guaranteed. Return or exchange any order within 30 days. Designed and sold by Hafeez Center in the United States.",
                    Image = "https://fakestoreapi.com/img/61sbMiUnoGL._AC_UL640_QL65_ML3_.jpg",
                    RatingRate = 3.9,
                    RatingCount = 70
                },
                new Product
                {
                    ProductId = 7,
                    ProductName = "WD 2TB Elements Portable External Hard Drive - USB 3.0",
                    Category = "electronics",
                    Price = 64.00m,
                    DiscountPercentage = 15.0m,
                    Description = "USB 3.0 and USB 2.0 Compatibility Fast data transfers Improve PC Performance High Capacity.",
                    Image = "https://fakestoreapi.com/img/61IBBVJvSDL._AC_SY879_.jpg",
                    RatingRate = 3.3,
                    RatingCount = 203
                },
                new Product
                {
                    ProductId = 8,
                    ProductName = "SanDisk SSD PLUS 1TB Internal SSD - SATA III 6 Gb/s",
                    Category = "electronics",
                    Price = 109.00m,
                    DiscountPercentage = 20.0m,
                    Description = "Easy upgrade for faster boot up, shutdown, application load and response. Boosts burst write performance, making it ideal for typical PC workloads.",
                    Image = "https://fakestoreapi.com/img/61U7T1koQqL._AC_SX679_.jpg",
                    RatingRate = 2.9,
                    RatingCount = 470
                }
            };
        }
    }
}
