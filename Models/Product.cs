// ==============================================================================
// Product.cs
// Namespace: OnlineShoppingSystem.Models
// Description: Core entity model representing a product in the Online Shopping System.
// Includes all properties specified in Requirement 1, plus rich display fields.
// ==============================================================================

using System.ComponentModel.DataAnnotations;

namespace OnlineShoppingSystem.Models
{
    /// <summary>
    /// Represents a product item within the Online Shopping Product Management System.
    /// This model holds all product attributes including identity, title, category,
    /// regular price, and discount percentage for dynamic price calculation.
    /// </summary>
    public class Product
    {
        // --------------------------------------------------------------------------
        // Requirement 1: ProductId
        // Description: Unique primary identifier for each product item.
        // Used in Query Strings (?id=...) and Hidden Form Fields.
        // --------------------------------------------------------------------------
        [Display(Name = "Product ID")]
        public int ProductId { get; set; }

        // --------------------------------------------------------------------------
        // Requirement 1: ProductName
        // Description: The name or title of the product item.
        // Required field with validation constraints.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150, ErrorMessage = "Product name cannot exceed 150 characters.")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Requirement 1: Category
        // Description: Department/classification of the product (e.g. electronics, clothing).
        // Used for filtering and stored in ASP.NET Core Session state.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Requirement 1: Price
        // Description: The standard retail/list price of the product in USD.
        // Must be a positive decimal value.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between $0.01 and $100,000.00.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Original Price ($)")]
        public decimal Price { get; set; }

        // --------------------------------------------------------------------------
        // Requirement 1: DiscountPercentage
        // Description: Discount percentage applied to the original price (0% to 100%).
        // Used by the GetDiscountedPrice() extension method to calculate selling price.
        // --------------------------------------------------------------------------
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0% and 100%.")]
        [Display(Name = "Discount (%)")]
        public decimal DiscountPercentage { get; set; } = 0.0m;

        // --------------------------------------------------------------------------
        // Supplementary Field: Description
        // Description: Detailed textual explanation of the product specifications.
        // --------------------------------------------------------------------------
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Supplementary Field: Image
        // Description: Direct URL to the product preview image (from FakeStoreAPI).
        // --------------------------------------------------------------------------
        [Display(Name = "Image URL")]
        public string Image { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Supplementary Field: RatingRate
        // Description: Average customer star rating (from 0.0 to 5.0).
        // --------------------------------------------------------------------------
        [Display(Name = "Customer Rating")]
        public double RatingRate { get; set; } = 4.5;

        // --------------------------------------------------------------------------
        // Supplementary Field: RatingCount
        // Description: Total count of customer reviews submitted for this product.
        // --------------------------------------------------------------------------
        [Display(Name = "Review Count")]
        public int RatingCount { get; set; } = 0;
    }
}
