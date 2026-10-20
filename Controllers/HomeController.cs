// ==============================================================================
// HomeController.cs
// Namespace: OnlineShoppingSystem.Controllers
// Description: Home controller managing application entry landing page,
// live system overview metrics, architecture documentation page, and error handler.
// ==============================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineShoppingSystem.Models;
using OnlineShoppingSystem.Services;

namespace OnlineShoppingSystem.Controllers
{
    /// <summary>
    /// Serves application entry pages, interactive architecture guide, and system health status.
    /// </summary>
    public class HomeController : Controller
    {
        // Private dependency injected services
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor for HomeController with injected dependencies.
        /// </summary>
        /// <param name="productService">Injected product service to query system stats</param>
        /// <param name="logger">Injected logger instance</param>
        public HomeController(IProductService productService, ILogger<HomeController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Renders the landing page with feature highlights and quick navigation.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Read customer name from Cookie if available
            string? customerName = Request.Cookies["CustomerName"];
            ViewBag.CustomerName = customerName;

            // Read category from Session if available
            string? selectedCategory = HttpContext.Session.GetString("SelectedCategory");
            ViewBag.SelectedCategory = selectedCategory;

            // Fetch product count and cache info for system status dashboard
            var (products, cacheStatus, timestamp) = await _productService.GetProductsAsync(forceRefresh: false);
            ViewBag.ProductCount = products.Count();
            ViewBag.CacheStatus = cacheStatus;
            ViewBag.CacheTimestamp = timestamp;

            return View(products.Take(4).ToList());
        }

        /// <summary>
        /// Interactive page detailing MVC architecture, flow diagrams, and state management mechanisms.
        /// </summary>
        [HttpGet]
        public IActionResult Architecture()
        {
            return View();
        }

        /// <summary>
        /// Privacy policy page.
        /// </summary>
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Global error handling action displaying request trace diagnostics.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
