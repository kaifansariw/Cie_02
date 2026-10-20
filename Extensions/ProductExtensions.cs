// ==============================================================================
// ProductExtensions.cs
// Namespace: OnlineShoppingSystem.Extensions
// Description: Static extension class providing discount calculations on Product model.
// Implements Requirement 1: GetDiscountedPrice() to calculate the final selling price.
// ==============================================================================

using OnlineShoppingSystem.Models;

namespace OnlineShoppingSystem.Extensions
{
    /// <summary>
    /// Extension methods class for the Product entity.
    /// Extension methods enable developers to "add" methods to existing types
    /// without modifying the original type or creating a derived type.
    /// </summary>
    public static class ProductExtensions
    {
        // --------------------------------------------------------------------------
        // Requirement 1: GetDiscountedPrice() Extension Method
        // Description: Calculates the net selling price after deducting the discount.
        // Formula: SellingPrice = OriginalPrice - (OriginalPrice * (DiscountPercentage / 100))
        // Parameters: this Product product (the instance on which extension is invoked)
        // Returns: decimal representing final discounted selling price rounded to 2 decimals.
        // --------------------------------------------------------------------------
        public static decimal GetDiscountedPrice(this Product product)
        {
            // Validate that the product reference is not null to prevent NullReferenceException
            if (product == null)
            {
                // Return zero if product is null
                return 0.0m;
            }

            // If discount percentage is 0 or negative, return the original price without reduction
            if (product.DiscountPercentage <= 0.0m)
            {
                // Return full original price
                return product.Price;
            }

            // Calculate the discount deduction amount: (Price * DiscountPercentage) / 100
            decimal discountDeduction = product.Price * (product.DiscountPercentage / 100.0m);

            // Compute the discounted selling price: Price - discountDeduction
            decimal sellingPrice = product.Price - discountDeduction;

            // Ensure the selling price never drops below zero
            if (sellingPrice < 0.0m)
            {
                // Selling price clamped to zero minimum
                sellingPrice = 0.0m;
            }

            // Return the calculated price rounded to two decimal places for currency formatting
            return Math.Round(sellingPrice, 2, MidpointRounding.AwayFromZero);
        }

        // --------------------------------------------------------------------------
        // Supplementary Extension: GetSavingsAmount()
        // Description: Calculates the monetary savings amount (in dollars) for the customer.
        // Formula: Savings = OriginalPrice - DiscountedPrice
        // Returns: decimal representing total money saved by customer.
        // --------------------------------------------------------------------------
        public static decimal GetSavingsAmount(this Product product)
        {
            // Null check validation
            if (product == null)
            {
                return 0.0m;
            }

            // Calculate difference between original price and discounted price
            decimal savings = product.Price - product.GetDiscountedPrice();

            // Return savings rounded to two decimal places
            return Math.Round(savings, 2, MidpointRounding.AwayFromZero);
        }

        // --------------------------------------------------------------------------
        // Supplementary Extension: HasDiscount()
        // Description: Helper method to determine if a product has an active discount.
        // Returns: true if DiscountPercentage is greater than 0; otherwise false.
        // --------------------------------------------------------------------------
        public static bool HasDiscount(this Product product)
        {
            // Return boolean indicating positive discount presence
            return product != null && product.DiscountPercentage > 0.0m;
        }
    }
}
