// ==============================================================================
// OrderViewModel.cs
// Namespace: OnlineShoppingSystem.Models
// Description: ViewModel representing an order checkout submission.
// Implements Requirement 3: Hidden Field submission of ProductId.
// ==============================================================================

using System.ComponentModel.DataAnnotations;

namespace OnlineShoppingSystem.Models
{
    /// <summary>
    /// ViewModel utilized by the Order Placement form.
    /// Demonstrates State Management Requirement 3: passing ProductId via a Hidden Field.
    /// Also captures customer details, calculates totals using GetDiscountedPrice(),
    /// and formats the order confirmation receipt.
    /// </summary>
    public class OrderViewModel
    {
        // --------------------------------------------------------------------------
        // Requirement 3: Hidden Field: Submit ProductId with an order form.
        // Description: Bound to <input type="hidden" asp-for="ProductId" />.
        // It persists the ProductId across the HTTP POST request without exposing
        // an editable input to the user.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Product ID is missing from the order submission.")]
        [Display(Name = "Product ID (Hidden Field)")]
        public int ProductId { get; set; }

        // --------------------------------------------------------------------------
        // CustomerName: Bound from the customer form, defaulting to Cookie value.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // CustomerEmail: Contact email for order dispatch notification.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Please enter a valid email address.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // ShippingAddress: Delivery physical address for the order shipment.
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Shipping address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // Quantity: Number of units requested (1 to 50).
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 50, ErrorMessage = "Quantity must be between 1 and 50 units.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        // --------------------------------------------------------------------------
        // PaymentMethod: Payment selection (Credit Card, PayPal, Cash on Delivery).
        // --------------------------------------------------------------------------
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Credit Card";

        // --------------------------------------------------------------------------
        // OrderReferenceNumber: Auto-generated unique tracking reference.
        // --------------------------------------------------------------------------
        [Display(Name = "Order Reference #")]
        public string OrderReferenceNumber { get; set; } = string.Empty;

        // --------------------------------------------------------------------------
        // OrderTimestamp: Timestamp recording when the order was placed.
        // --------------------------------------------------------------------------
        [Display(Name = "Order Date & Time")]
        public DateTime OrderTimestamp { get; set; } = DateTime.UtcNow;

        // --------------------------------------------------------------------------
        // UnitPrice: Original product list price.
        // --------------------------------------------------------------------------
        [Display(Name = "Unit List Price")]
        public decimal UnitPrice { get; set; }

        // --------------------------------------------------------------------------
        // DiscountPercentage: Percentage reduction applied.
        // --------------------------------------------------------------------------
        [Display(Name = "Discount Percentage")]
        public decimal DiscountPercentage { get; set; }

        // --------------------------------------------------------------------------
        // UnitDiscountedPrice: Calculated selling price per unit using GetDiscountedPrice().
        // --------------------------------------------------------------------------
        [Display(Name = "Discounted Selling Price")]
        public decimal UnitDiscountedPrice { get; set; }

        // --------------------------------------------------------------------------
        // TotalAmount: Final bill = UnitDiscountedPrice * Quantity.
        // --------------------------------------------------------------------------
        [Display(Name = "Grand Total")]
        public decimal TotalAmount { get; set; }

        // --------------------------------------------------------------------------
        // TotalSavings: Savings amount = (UnitPrice - UnitDiscountedPrice) * Quantity.
        // --------------------------------------------------------------------------
        [Display(Name = "Total Money Saved")]
        public decimal TotalSavings { get; set; }

        // --------------------------------------------------------------------------
        // Product: Attached Product instance for UI rendering on confirmation page.
        // --------------------------------------------------------------------------
        public Product? Product { get; set; }
    }
}
