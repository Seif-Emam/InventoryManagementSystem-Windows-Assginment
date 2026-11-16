using System;
using System.Globalization;

namespace InventoryManagementSystem.Validation
{
    /// <summary>
    /// Centralized domain validator for Inventory Products.
    /// Used by both ProductForm (GUI) and TerminalCommandProcessor (CLI)
    /// to guarantee uniform validation rules, strict business constraints, and feature-related error feedback.
    /// </summary>
    public static class ProductValidator
    {
        public const int MaxNameLength = 100;
        public const decimal MaxPrice = 1_000_000m;
        public const int MaxQuantity = 1_000_000;
        public const int MaxMinStock = 100_000;

        /// <summary>
        /// Validates product name according to assignment business rules.
        /// </summary>
        public static ValidationResult ValidateName(string? name, string feature = "Product Creation")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Failure(
                    feature,
                    "Product Name",
                    name,
                    "Product name cannot be empty or whitespace.",
                    "Enter a descriptive product name (e.g., 'Logitech MX Master 3S')."
                );
            }

            string trimmed = name.Trim();
            if (trimmed.Length > MaxNameLength)
            {
                return ValidationResult.Failure(
                    feature,
                    "Product Name",
                    trimmed,
                    $"Product name cannot exceed {MaxNameLength} characters (currently {trimmed.Length}).",
                    "Please shorten the product name to 100 characters or fewer."
                );
            }

            return ValidationResult.Success(feature);
        }

        /// <summary>
        /// Validates product price string and parses it to decimal.
        /// </summary>
        public static ValidationResult ValidatePrice(string? priceText, out decimal price, string feature = "Product Creation")
        {
            price = 0m;

            if (string.IsNullOrWhiteSpace(priceText))
            {
                return ValidationResult.Failure(
                    feature,
                    "Price",
                    priceText,
                    "Product price is required.",
                    "Enter a valid numeric dollar amount (e.g., 49.99)."
                );
            }

            string cleaned = priceText.Trim().TrimStart('$');
            if (!decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price) &&
                !decimal.TryParse(cleaned, out price))
            {
                return ValidationResult.Failure(
                    feature,
                    "Price",
                    priceText,
                    "Price must be a valid numeric dollar value.",
                    "Ensure the price contains only digits and an optional decimal point (e.g., 29.99)."
                );
            }

            return ValidatePriceValue(price, feature);
        }

        /// <summary>
        /// Validates an already parsed price decimal value.
        /// </summary>
        public static ValidationResult ValidatePriceValue(decimal? price, string feature = "Product Creation")
        {
            if (!price.HasValue)
            {
                return ValidationResult.Failure(
                    feature,
                    "Price",
                    null,
                    "Product price is required.",
                    "Provide a valid price value (e.g., --price 39.99)."
                );
            }

            if (price.Value < 0)
            {
                return ValidationResult.Failure(
                    feature,
                    "Price",
                    price.Value.ToString("F2", CultureInfo.InvariantCulture),
                    "Price cannot be negative. Must be zero or greater.",
                    "Enter a non-negative amount (e.g., 0.00 for free promotional items or positive price)."
                );
            }

            if (price.Value > MaxPrice)
            {
                return ValidationResult.Failure(
                    feature,
                    "Price",
                    price.Value.ToString("F2", CultureInfo.InvariantCulture),
                    $"Price cannot exceed ${MaxPrice:N2}.",
                    "Verify the price is within standard inventory limits."
                );
            }

            return ValidationResult.Success(feature);
        }

        /// <summary>
        /// Validates product quantity string and parses it to integer.
        /// </summary>
        public static ValidationResult ValidateQuantity(string? qtyText, out int quantity, string feature = "Product Creation")
        {
            quantity = 0;

            if (string.IsNullOrWhiteSpace(qtyText))
            {
                return ValidationResult.Failure(
                    feature,
                    "Stock Quantity",
                    qtyText,
                    "Stock quantity is required.",
                    "Enter an integer for available stock (e.g., 20)."
                );
            }

            string cleaned = qtyText.Trim();
            if (!int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity) &&
                !int.TryParse(cleaned, out quantity))
            {
                return ValidationResult.Failure(
                    feature,
                    "Stock Quantity",
                    qtyText,
                    "Stock quantity must be a whole non-negative integer.",
                    "Decimals, negative numbers, and letters are not allowed (e.g., use 10, not 10.5)."
                );
            }

            return ValidateQuantityValue(quantity, feature);
        }

        /// <summary>
        /// Validates an already parsed quantity integer value.
        /// </summary>
        public static ValidationResult ValidateQuantityValue(int? quantity, string feature = "Product Creation")
        {
            if (!quantity.HasValue)
            {
                return ValidationResult.Failure(
                    feature,
                    "Stock Quantity",
                    null,
                    "Stock quantity is required.",
                    "Provide a valid stock quantity (e.g., --stock 15)."
                );
            }

            if (quantity.Value < 0)
            {
                return ValidationResult.Failure(
                    feature,
                    "Stock Quantity",
                    quantity.Value.ToString(),
                    "Stock quantity cannot be negative.",
                    "Use 0 for out-of-stock items, or a positive integer."
                );
            }

            if (quantity.Value > MaxQuantity)
            {
                return ValidationResult.Failure(
                    feature,
                    "Stock Quantity",
                    quantity.Value.ToString(),
                    $"Stock quantity cannot exceed {MaxQuantity:N0} units.",
                    "Verify quantity value is within allowable inventory range."
                );
            }

            return ValidationResult.Success(feature);
        }

        /// <summary>
        /// Validates minimum stock threshold string and parses it to integer.
        /// </summary>
        public static ValidationResult ValidateMinStock(string? minStockText, out int minStock, string feature = "Product Creation")
        {
            minStock = 5;

            if (string.IsNullOrWhiteSpace(minStockText))
            {
                return ValidationResult.Failure(
                    feature,
                    "Minimum Stock",
                    minStockText,
                    "Minimum stock threshold is required.",
                    "Enter a whole integer threshold (e.g., 5)."
                );
            }

            string cleaned = minStockText.Trim();
            if (!int.TryParse(cleaned, NumberStyles.Integer, CultureInfo.InvariantCulture, out minStock) &&
                !int.TryParse(cleaned, out minStock))
            {
                return ValidationResult.Failure(
                    feature,
                    "Minimum Stock",
                    minStockText,
                    "Minimum stock threshold must be a whole non-negative integer.",
                    "Decimals, negative numbers, and letters are not allowed (e.g., use 5)."
                );
            }

            return ValidateMinStockValue(minStock, feature);
        }

        /// <summary>
        /// Validates an already parsed minimum stock integer value.
        /// </summary>
        public static ValidationResult ValidateMinStockValue(int? minStock, string feature = "Product Creation")
        {
            if (!minStock.HasValue)
            {
                return ValidationResult.Failure(
                    feature,
                    "Minimum Stock",
                    null,
                    "Minimum stock threshold is required.",
                    "Provide a minimum stock threshold (e.g., --min 5)."
                );
            }

            if (minStock.Value < 0)
            {
                return ValidationResult.Failure(
                    feature,
                    "Minimum Stock",
                    minStock.Value.ToString(),
                    "Minimum stock threshold cannot be negative.",
                    "Enter 0 or a positive whole integer."
                );
            }

            if (minStock.Value > MaxMinStock)
            {
                return ValidationResult.Failure(
                    feature,
                    "Minimum Stock",
                    minStock.Value.ToString(),
                    $"Minimum stock threshold cannot exceed {MaxMinStock:N0} units.",
                    "Verify threshold value is within allowable bounds."
                );
            }

            return ValidationResult.Success(feature);
        }

        /// <summary>
        /// Validates all fields for Product Creation from raw string inputs.
        /// Returns the first validation error encountered, or Success.
        /// </summary>
        public static ValidationResult ValidateCreation(
            string? name,
            string? priceText,
            string? qtyText,
            string? minStockText,
            out decimal parsedPrice,
            out int parsedQty,
            out int parsedMinStock,
            string feature = "Product Creation")
        {
            parsedPrice = 0m;
            parsedQty = 0;
            parsedMinStock = 5;

            var nameRes = ValidateName(name, feature);
            if (!nameRes.IsValid) return nameRes;

            var priceRes = ValidatePrice(priceText, out parsedPrice, feature);
            if (!priceRes.IsValid) return priceRes;

            var qtyRes = ValidateQuantity(qtyText, out parsedQty, feature);
            if (!qtyRes.IsValid) return qtyRes;

            var minRes = ValidateMinStock(minStockText, out parsedMinStock, feature);
            if (!minRes.IsValid) return minRes;

            return ValidationResult.Success(feature);
        }

        /// <summary>
        /// Validates all fields for Product Creation from typed values.
        /// </summary>
        public static ValidationResult ValidateCreationValues(
            string? name,
            decimal? price,
            int? quantity,
            int? minStock,
            string feature = "Product Creation")
        {
            var nameRes = ValidateName(name, feature);
            if (!nameRes.IsValid) return nameRes;

            var priceRes = ValidatePriceValue(price, feature);
            if (!priceRes.IsValid) return priceRes;

            var qtyRes = ValidateQuantityValue(quantity, feature);
            if (!qtyRes.IsValid) return qtyRes;

            if (minStock.HasValue)
            {
                var minRes = ValidateMinStockValue(minStock, feature);
                if (!minRes.IsValid) return minRes;
            }

            return ValidationResult.Success(feature);
        }
    }
}

