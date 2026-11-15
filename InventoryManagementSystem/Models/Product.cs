using System;

namespace InventoryManagementSystem.Models
{
    /// <summary>
    /// Represents an inventory product.
    /// StockStatus is calculated dynamically based on Quantity and MinStock business rules.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; }

        /// <summary>
        /// Calculated stock status based on assignment business rules:
        /// 1. Quantity == 0 -> "Out of Stock"
        /// 2. Quantity > 0 && Quantity <= MinStock -> "Low Stock"
        /// 3. Otherwise -> "Available"
        /// </summary>
        public string StockStatus
        {
            get
            {
                if (Quantity == 0)
                    return "Out of Stock";
                if (Quantity > 0 && Quantity <= MinStock)
                    return "Low Stock";
                return "Available";
            }
        }

        public Product()
        {
        }

        public Product(int id, string name, decimal price, int quantity, int minStock)
        {
            Id = id;
            Name = name;
            Price = price;
            Quantity = quantity;
            MinStock = minStock;
        }

        /// <summary>
        /// Creates a shallow clone of the product for edit operations.
        /// </summary>
        public Product Clone()
        {
            return new Product
            {
                Id = this.Id,
                Name = this.Name,
                Price = this.Price,
                Quantity = this.Quantity,
                MinStock = this.MinStock
            };
        }
    }
}

