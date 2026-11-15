using System;
using System.Collections.Generic;
using System.IO;
using InventoryManagementSystem.Models;
using Microsoft.Data.Sqlite;

namespace InventoryManagementSystem.Data
{
    /// <summary>
    /// Handles SQLite database creation, schema initialization, and idempotent seeding.
    /// Ensures that persistent storage is maintained in %LOCALAPPDATA%/InventoryManagementSystem/inventory.db.
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Default persistent SQLite database path in %LOCALAPPDATA%/InventoryManagementSystem/inventory.db.
        /// Stored outside of bin/Debug and temporary directories to survive application rebuilds.
        /// </summary>
        public static string DefaultDatabasePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InventoryManagementSystem",
                "inventory.db"
            );

        /// <summary>
        /// Generates a SQLite connection string for the specified or default database path.
        /// </summary>
        public static string GetConnectionString(string? dbPath = null)
        {
            string path = string.IsNullOrWhiteSpace(dbPath) ? DefaultDatabasePath : dbPath;
            return $"Data Source={path};";
        }

        /// <summary>
        /// Initializes the database directory, schema table, and performs idempotent initial seeding.
        /// </summary>
        public static void Initialize(string? dbPath = null)
        {
            string path = string.IsNullOrWhiteSpace(dbPath) ? DefaultDatabasePath : dbPath;

            // 1. Automatically create directory if it does not exist
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string connectionString = GetConnectionString(path);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // 2. Create Products table if not exists with constraints
            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL CHECK(Price >= 0),
                    Quantity INTEGER NOT NULL CHECK(Quantity >= 0),
                    MinStock INTEGER NOT NULL CHECK(MinStock >= 0)
                );";

            using (var command = new SqliteCommand(createTableSql, connection))
            {
                command.ExecuteNonQuery();
            }

            // 3. Idempotent Seeding: Only seed when table is completely empty
            const string countSql = "SELECT COUNT(*) FROM Products;";
            long existingCount = 0;
            using (var countCmd = new SqliteCommand(countSql, connection))
            {
                var result = countCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    existingCount = Convert.ToInt64(result);
                }
            }

            // If products already exist, never overwrite, duplicate, or reseed!
            if (existingCount == 0)
            {
                SeedInitialProducts(connection);
            }
        }

        /// <summary>
        /// Inserts curated realistic initial products inside a single database transaction.
        /// </summary>
        private static void SeedInitialProducts(SqliteConnection connection)
        {
            var seedProducts = GetSeedProducts();

            using var transaction = connection.BeginTransaction();
            try
            {
                const string insertSql = @"
                    INSERT INTO Products (Name, Price, Quantity, MinStock)
                    VALUES (@Name, @Price, @Quantity, @MinStock);";

                using var insertCmd = new SqliteCommand(insertSql, connection, transaction);
                var paramName = insertCmd.Parameters.Add("@Name", SqliteType.Text);
                var paramPrice = insertCmd.Parameters.Add("@Price", SqliteType.Real);
                var paramQuantity = insertCmd.Parameters.Add("@Quantity", SqliteType.Integer);
                var paramMinStock = insertCmd.Parameters.Add("@MinStock", SqliteType.Integer);

                foreach (var product in seedProducts)
                {
                    paramName.Value = product.Name;
                    paramPrice.Value = (double)product.Price;
                    paramQuantity.Value = product.Quantity;
                    paramMinStock.Value = product.MinStock;

                    insertCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Returns 25 realistic demonstration products covering varied computer and office electronics categories.
        /// Includes products in Available, Low Stock, and Out of Stock conditions.
        /// </summary>
        public static IReadOnlyList<Product> GetSeedProducts()
        {
            return new List<Product>
            {
                // Keyboards & Mice
                new Product(0, "Logitech MX Master 3S Mouse", 99.99m, 18, 5),
                new Product(0, "Logitech K380 Wireless Keyboard", 39.99m, 12, 4),
                new Product(0, "SteelSeries Rival 3 Gaming Mouse", 29.99m, 22, 5),
                new Product(0, "Keychron K2 Mechanical Keyboard", 79.99m, 10, 3),

                // Monitors
                new Product(0, "Dell 24-inch FHD Monitor", 159.99m, 8, 3),
                new Product(0, "LG UltraGear 27-inch QHD Monitor", 279.99m, 5, 2),

                // Storage Devices
                new Product(0, "Samsung 990 EVO SSD 1TB", 89.99m, 15, 5),
                new Product(0, "Crucial P3 Plus M.2 NVMe SSD 2TB", 119.99m, 3, 4), // Low Stock
                new Product(0, "SanDisk Ultra USB 3.0 Flash Drive 128GB", 14.99m, 45, 10),
                new Product(0, "Seagate BarraCuda 2TB Internal HDD", 54.99m, 11, 4),

                // Computer Components
                new Product(0, "Kingston Fury DDR5 RAM 16GB", 64.99m, 2, 4), // Low Stock
                new Product(0, "ASUS Dual GeForce RTX 4060 GPU", 329.99m, 1, 2), // Low Stock
                new Product(0, "Corsair RM750e 750W Power Supply", 99.99m, 7, 2),
                new Product(0, "Noctua NH-D15 CPU Air Cooler", 109.99m, 3, 3), // Low Stock (Qty == MinStock)

                // Networking Equipment
                new Product(0, "TP-Link Archer AX55 WiFi 6 Router", 79.99m, 6, 3),
                new Product(0, "Netgear 8-Port Gigabit Ethernet Switch", 24.99m, 19, 5),

                // Audio Equipment & Headsets
                new Product(0, "HyperX Cloud III Wireless Headset", 129.99m, 0, 2), // Out of Stock
                new Product(0, "Sony WH-1000XM5 Noise Canceling Headphones", 349.99m, 4, 2),
                new Product(0, "Blue Yeti USB Microphone", 99.99m, 0, 3), // Out of Stock

                // Computer Accessories & Office Electronics
                new Product(0, "Logitech C920 Pro HD Webcam", 69.99m, 0, 3), // Out of Stock
                new Product(0, "Anker PowerExpand 8-in-1 USB-C Hub", 49.99m, 14, 4),
                new Product(0, "Apple Magic Trackpad", 129.99m, 0, 2), // Out of Stock
                new Product(0, "Epson EcoTank Wireless Color Printer", 249.99m, 6, 2),
                new Product(0, "APC Back-UPS 600VA Battery Backup", 74.99m, 9, 3),
                new Product(0, "Elgato Stream Deck MK.2", 149.99m, 8, 2)
            };
        }
    }
}

