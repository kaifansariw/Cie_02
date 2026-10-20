// ==============================================================================
// Program.cs
// Application: Online Shopping Product Management System
// Framework: ASP.NET Core MVC (.NET 10)
// Description: Application entry point and configuration for Dependency Injection (DI)
// container, middleware pipeline, Session state, Response Caching, Memory Caching,
// and HttpClient services.
// ==============================================================================

using OnlineShoppingSystem.Services;

// Create the WebApplicationBuilder which configures configuration, logging, and services
var builder = WebApplication.CreateBuilder(args);

// ==============================================================================
// 1. Dependency Injection Service Registrations
// ==============================================================================

// Register MVC Controller and Razor Views services into the IoC container
builder.Services.AddControllersWithViews();

// Register In-Memory Caching (IMemoryCache) for Requirement 4
// Used by ProductService to cache external API responses with a 10-minute expiration
builder.Services.AddMemoryCache();

// Register Response Caching services for Requirement 4
// Enables the [ResponseCache] attribute on Controller actions (e.g. Product Details)
builder.Services.AddResponseCaching();

// Register In-Memory Distributed Cache provider required for Session state backing store
builder.Services.AddDistributedMemoryCache();

// Register Session State services for Requirement 3 (Session State: Category filtering)
builder.Services.AddSession(options =>
{
    // Set session cookie timeout duration to 30 minutes of idle inactivity
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // Enforce HttpOnly on session cookie to protect against client-side script tampering
    options.Cookie.HttpOnly = true;
    // Mark session cookie as essential for GDPR compliance
    options.Cookie.IsEssential = true;
    // Custom name for the session cookie
    options.Cookie.Name = ".OnlineShopping.Session";
});

// Register typed HttpClient for IProductService for Requirement 2 (Async Web Request)
// Automatically manages HttpClient pooling, socket reuse, and lifetime
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    // Set standard request headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "OnlineShoppingProductManagementSystem/1.0");
    // Timeout safeguard for external web calls (10 seconds)
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Build the configured WebApplication instance
var app = builder.Build();

// ==============================================================================
// 2. HTTP Request Pipeline (Middleware Configuration)
// ==============================================================================

// Configure the HTTP request pipeline for production vs development environments
if (!app.Environment.IsDevelopment())
{
    // Redirect unhandled exceptions to global Error action in production
    app.UseExceptionHandler("/Home/Error");
    // Enforce HTTP Strict Transport Security (HSTS)
    app.UseHsts();
}

// Redirect standard HTTP traffic to secure HTTPS automatically
app.UseHttpsRedirection();

// Serve static assets (CSS, JavaScript, images) from wwwroot folder
app.UseStaticFiles();

// Enable endpoint routing to match incoming requests to controller actions
app.UseRouting();

// Enable Response Caching middleware (Requirement 4)
// Must be placed after UseRouting() and before UseEndpoints()/MapControllerRoute()
app.UseResponseCaching();

// Enable Session middleware (Requirement 3)
// Must be invoked before UseAuthorization() and endpoint execution
app.UseSession();

// Enable authorization middleware
app.UseAuthorization();

// Configure the default conventional MVC route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Execute and start listening for incoming HTTP requests
app.Run();
