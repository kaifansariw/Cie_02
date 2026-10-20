// ==============================================================================
// ProductController.cs
// Namespace: OnlineShoppingSystem.Controllers
// Description: Core controller managing product display, state management
// (Cookies, Sessions, Query Strings, Hidden Fields), asynchronous web calls,
// extension methods, and caching (ResponseCache & IMemoryCache).
// ==============================================================================

using Microsoft.AspNetCore.Mvc;
using OnlineShoppingSystem.Extensions;
using OnlineShoppingSystem.Models;
using OnlineShoppingSystem.Services;

namespace OnlineShoppingSystem.Controllers
{
    /// <summary>
    /// Controller responsible for handling user interactions with the product catalog,
    /// product details, order placement, and demonstration of state management & caching.
    /// </summary>
    public class ProductController : Controller
    {
        // --------------------------------------------------------------------------
        // Private Dependency Injected Services
        // --------------------------------------------------------------------------
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        // Constants for State Management Keys
        private const string CustomerNameCookieKey = "CustomerName";
        private const string SelectedCategorySessionKey = "SelectedCategory";

        /// <summary>
        /// Constructor injecting the ProductService and Logger.
        /// </summary>
        /// <param name="productService">Injected service for async product retrieval & memory cache</param>
        /// <param name="logger">Injected logger instance</param>
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==========================================================================
        // Requirement 2: Asynchronous Web Request
        // Requirement 3: State Management (Session: Category & Cookie: Customer Name)
        // Requirement 4: Caching (IMemoryCache with 10-minute expiration)
        // ==========================================================================
        /// <summary>
        /// Displays the catalog of products in a responsive table and card view.
        /// Demonstrates:
        /// - Asynchronous web request to FakeStoreAPI
        /// - IMemoryCache 10-minute storage
        /// - Session state retrieval for category filter
        /// - Cookie state retrieval for customer name
        /// </summary>
        /// <param name="category">Optional category parameter from request</param>
        /// <param name="search">Optional keyword search string</param>
        /// <param name="forceRefresh">Flag to manually bypass memory cache and re-fetch</param>
        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? search, bool forceRefresh = false)
        {
            // ----------------------------------------------------------------------
            // Requirement 3 (Session State): Category Persistence
            // If category is provided in query, update the Session.
            // If category is not provided, check if a category was previously stored in Session.
            // ----------------------------------------------------------------------
            if (!string.IsNullOrEmpty(category))
            {
                if (category.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // Clear category filter from Session
                    HttpContext.Session.Remove(SelectedCategorySessionKey);
                    category = null;
                }
                else
                {
                    // Store the selected category inside ASP.NET Core Session state
                    HttpContext.Session.SetString(SelectedCategorySessionKey, category);
                }
            }
            else
            {
                // Retrieve the previously selected category from Session state if available
                category = HttpContext.Session.GetString(SelectedCategorySessionKey);
            }

            // ----------------------------------------------------------------------
            // Requirement 3 (Cookie State): Read Customer Name from Request Cookies
            // ----------------------------------------------------------------------
            string? customerName = Request.Cookies[CustomerNameCookieKey];

            // ----------------------------------------------------------------------
            // Requirement 2 & 4: Fetch products asynchronously with 10-min Memory Cache
            // ----------------------------------------------------------------------
            var (allProducts, cacheStatus, cacheTimestamp) = await _productService.GetProductsAsync(forceRefresh);

            // Retrieve all distinct categories for rendering filter pills/dropdown
            var categories = await _productService.GetCategoriesAsync();

            // Filter products by selected category if active
            var filteredProducts = allProducts;
            if (!string.IsNullOrEmpty(category))
            {
                filteredProducts = filteredProducts.Where(p =>
                    string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
            }

            // Filter products by search keyword if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Build ViewModel containing all required view data
            var viewModel = new ProductCatalogViewModel
            {
                Products = filteredProducts.ToList(),
                Categories = categories,
                SelectedCategory = category,
                CustomerName = customerName,
                CacheStatus = cacheStatus,
                CacheTimestamp = cacheTimestamp,
                SearchTerm = search,
                TotalProductCount = filteredProducts.Count()
            };

            return View(viewModel);
        }

        // ==========================================================================
        // Requirement 1: Model and Extension Method (Display discounted price)
        // Requirement 3: State Management (Query String: Pass ProductId to Details)
        // Requirement 4: Caching (Response Caching for Product Details page)
        // ==========================================================================
        /// <summary>
        /// Displays full details for a single product.
        /// Demonstrates:
        /// - Query String: Reads ProductId from the URL (?id=...)
        /// - Extension Method: Calls product.GetDiscountedPrice() to calculate selling price
        /// - Response Caching: Cached on HTTP client / proxies for 60 seconds, varies by 'id'
        /// </summary>
        /// <param name="id">Product identifier passed via query string (?id=1)</param>
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
        public async Task<IActionResult> Details([FromQuery] int? id)
        {
            // Explicitly demonstrate reading from Query String via Request.Query collection
            string? queryStringId = Request.Query["id"];
            _logger.LogInformation("Product Details requested. Extracted Query String 'id': {QueryId}", queryStringId);

            // Validate that ProductId was provided in the query string
            if (!id.HasValue || id.Value <= 0)
            {
                // Return a descriptive error view if query string was omitted or invalid
                ViewBag.ErrorMessage = "Invalid or missing Product ID in query string. Example usage: /Product/Details?id=1";
                return View("ErrorDetails");
            }

            // Asynchronously retrieve the product item by ID
            var product = await _productService.GetProductByIdAsync(id.Value);

            if (product == null)
            {
                // Return 404 Not Found if product doesn't exist
                return NotFound($"Product with ID {id.Value} was not found.");
            }

            // ----------------------------------------------------------------------
            // Requirement 1: Invoke Extension Method GetDiscountedPrice()
            // ----------------------------------------------------------------------
            decimal discountedPrice = product.GetDiscountedPrice();
            decimal savingsAmount = product.GetSavingsAmount();

            // Pass calculated prices to ViewBag / ViewData for UI inspection
            ViewBag.CalculatedDiscountedPrice = discountedPrice;
            ViewBag.CalculatedSavings = savingsAmount;
            ViewBag.RenderTimestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff UTC");

            // Read customer name from Cookie if present to prefill the order form
            string? customerName = Request.Cookies[CustomerNameCookieKey] ?? string.Empty;

            // Prepare OrderViewModel pre-loaded with Hidden Field ProductId
            var orderForm = new OrderViewModel
            {
                // Requirement 3: Hidden Field ProductId bound here
                ProductId = product.ProductId,
                CustomerName = customerName,
                UnitPrice = product.Price,
                DiscountPercentage = product.DiscountPercentage,
                UnitDiscountedPrice = discountedPrice,
                Product = product,
                Quantity = 1
            };

            ViewBag.OrderForm = orderForm;

            return View(product);
        }

        // ==========================================================================
        // Requirement 3: State Management (Hidden Field: Submit ProductId with Order)
        // ==========================================================================
        /// <summary>
        /// Processes order placement submitted from the product details form.
        /// Demonstrates:
        /// - Hidden Field: Extracts ProductId submitted in the form payload
        /// - Extension Method: Computes total order amounts and customer savings
        /// - Cookie Synchronization: Updates CustomerName cookie if customer provided a new name
        /// </summary>
        /// <param name="orderModel">Order data bound from the HTTP POST form</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(OrderViewModel orderModel)
        {
            // Explicitly demonstrate hidden field extraction from Form collection
            string? hiddenFieldProductId = Request.Form["ProductId"];
            _logger.LogInformation("Order received. Hidden Field ProductId: {HiddenId}", hiddenFieldProductId);

            // Fetch the product to verify validity and recalculate authoritative price
            var product = await _productService.GetProductByIdAsync(orderModel.ProductId);

            if (product == null)
            {
                ModelState.AddModelError(string.Empty, "The selected product is invalid or no longer available.");
                return RedirectToAction(nameof(Index));
            }

            // Re-attach product to model
            orderModel.Product = product;
            orderModel.UnitPrice = product.Price;
            orderModel.DiscountPercentage = product.DiscountPercentage;

            // ----------------------------------------------------------------------
            // Requirement 1: Compute discounted selling price using extension method
            // ----------------------------------------------------------------------
            orderModel.UnitDiscountedPrice = product.GetDiscountedPrice();
            orderModel.TotalAmount = orderModel.UnitDiscountedPrice * orderModel.Quantity;
            orderModel.TotalSavings = product.GetSavingsAmount() * orderModel.Quantity;
            orderModel.OrderTimestamp = DateTime.UtcNow;
            orderModel.OrderReferenceNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            // If model validation fails, return to Details view with validation feedback
            if (!ModelState.IsValid)
            {
                ViewBag.CalculatedDiscountedPrice = orderModel.UnitDiscountedPrice;
                ViewBag.CalculatedSavings = product.GetSavingsAmount();
                ViewBag.OrderForm = orderModel;
                return View("Details", product);
            }

            // ----------------------------------------------------------------------
            // Requirement 3: Cookie State Management
            // Update the CustomerName cookie so subsequent visits remember this customer
            // ----------------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(orderModel.CustomerName))
            {
                var cookieOptions = new CookieOptions
                {
                    // Expire cookie in 7 days
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    // Allow client-side JavaScript access for interactive UI elements
                    HttpOnly = false,
                    // Secure in production HTTPS environments
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                };
                Response.Cookies.Append(CustomerNameCookieKey, orderModel.CustomerName, cookieOptions);
            }

            // Render order confirmation view displaying receipt details
            return View("OrderConfirmation", orderModel);
        }

        // ==========================================================================
        // Requirement 3: State Management (Cookie helper actions)
        // ==========================================================================
        /// <summary>
        /// Sets the CustomerName cookie from a simple modal or form.
        /// </summary>
        /// <param name="customerName">Customer's name to remember in cookie</param>
        /// <param name="returnUrl">URL to return to after setting cookie</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetCustomerCookie(string customerName, string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = false,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                };

                // Append cookie to HTTP response headers
                Response.Cookies.Append(CustomerNameCookieKey, customerName.Trim(), cookieOptions);
                _logger.LogInformation("Saved CustomerName cookie: {Name}", customerName);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Deletes the CustomerName cookie.
        /// </summary>
        /// <param name="returnUrl">URL to return to after deleting cookie</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCustomerCookie(string? returnUrl)
        {
            // Expire and remove cookie
            Response.Cookies.Delete(CustomerNameCookieKey);
            _logger.LogInformation("CustomerName cookie deleted.");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================================================
        // Requirement 3: State Management (Session helper actions)
        // ==========================================================================
        /// <summary>
        /// Resets the selected category stored in Session state.
        /// </summary>
        [HttpGet]
        public IActionResult ClearCategorySession()
        {
            HttpContext.Session.Remove(SelectedCategorySessionKey);
            return RedirectToAction(nameof(Index));
        }

        // ==========================================================================
        // Requirement 4: Caching helper action (Force Refresh Memory Cache)
        // ==========================================================================
        /// <summary>
        /// Manually purges IMemoryCache entry and re-fetches products live from FakeStoreAPI.
        /// Allows testing of cache expiry behavior on demand.
        /// </summary>
        [HttpGet]
        public IActionResult RefreshCache()
        {
            _productService.ClearProductCache();
            TempData["SuccessMessage"] = "Memory cache cleared! Products will now be fetched live from FakeStoreAPI.";
            return RedirectToAction(nameof(Index), new { forceRefresh = true });
        }
    }
}
