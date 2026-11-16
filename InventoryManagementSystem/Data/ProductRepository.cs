using System;
using System.Collections.Generic;
using InventoryManagementSystem.Models;
using Microsoft.Data.Sqlite;

namespace InventoryManagementSystem.Data
{
    /// <summary>
    /// SQLite Data Access Layer for Product management.
    /// Uses parameterized SQL queries, ensures proper connection disposal,
    /// and serves as the single persistent source of truth for the inventory system.
    /// </summary>
    public class ProductRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes the repository using the specified database path or the default %LOCALAPPDATA% path.
        /// </summary>
        public ProductRepository(string? dbPath = null)
        {
            _connectionString = DatabaseInitializer.GetConnectionString(dbPath);
        }

        private SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Retrieves all products ordered by ID.
        /// </summary>
        public List<Product> GetAll()
        {
            try
            {
                var list = new List<Product>();
                const string sql = "SELECT Id, Name, Price, Quantity, MinStock FROM Products ORDER BY Id ASC;";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(ReadProduct(reader));
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Inventory Catalog", "Query All Products", ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves a single product by its unique ID.
        /// </summary>
        public Product? GetById(int id)
        {
            try
            {
                const string sql = "SELECT Id, Name, Price, Quantity, MinStock FROM Products WHERE Id = @Id;";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return ReadProduct(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Product Details", $"Get Product #{id}", ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserts a new product into SQLite using parameterized query and sets its generated ID.
        /// </summary>
        public Product Add(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                const string sql = @"
                    INSERT INTO Products (Name, Price, Quantity, MinStock)
                    VALUES (@Name, @Price, @Quantity, @MinStock);
                    SELECT last_insert_rowid();";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);

                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Price", (double)product.Price);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@MinStock", product.MinStock);

                object? result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    product.Id = Convert.ToInt32(result);
                }

                return product;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Product Creation", $"Insert Product '{product.Name}'", ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates an existing product in SQLite.
        /// </summary>
        public bool Update(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                const string sql = @"
                    UPDATE Products
                    SET Name = @Name,
                        Price = @Price,
                        Quantity = @Quantity,
                        MinStock = @MinStock
                    WHERE Id = @Id;";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);

                command.Parameters.AddWithValue("@Id", product.Id);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Price", (double)product.Price);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@MinStock", product.MinStock);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Product Editing", $"Update Product #{product.Id}", ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes a product from SQLite by ID.
        /// </summary>
        public bool Delete(int id)
        {
            try
            {
                const string sql = "DELETE FROM Products WHERE Id = @Id;";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Product Deletion", $"Delete Product #{id}", ex.Message, ex);
            }
        }

        /// <summary>
        /// Live search by partial Name (case-insensitive) or partial ID.
        /// </summary>
        public List<Product> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return GetAll();
            }

            try
            {
                var list = new List<Product>();
                const string sql = @"
                    SELECT Id, Name, Price, Quantity, MinStock
                    FROM Products
                    WHERE Name LIKE @Query OR CAST(Id AS TEXT) LIKE @Query
                    ORDER BY Id ASC;";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@Query", $"%{query.Trim()}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadProduct(reader));
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Inventory Search", $"Search with query '{query}'", ex.Message, ex);
            }
        }

        /// <summary>
        /// Calculates dashboard statistics directly from SQLite data based on dynamic business rules.
        /// </summary>
        public (int Total, int LowStock, int OutOfStock) GetDashboardStatistics()
        {
            try
            {
                const string sql = @"
                    SELECT 
                        COUNT(*) AS Total,
                        TOTAL(CASE WHEN Quantity > 0 AND Quantity <= MinStock THEN 1 ELSE 0 END) AS LowStock,
                        TOTAL(CASE WHEN Quantity = 0 THEN 1 ELSE 0 END) AS OutOfStock
                    FROM Products;";

                using var connection = CreateConnection();
                using var command = new SqliteCommand(sql, connection);
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int total = Convert.ToInt32(reader["Total"]);
                    int lowStock = Convert.ToInt32(reader["LowStock"]);
                    int outOfStock = Convert.ToInt32(reader["OutOfStock"]);
                    return (total, lowStock, outOfStock);
                }

                return (0, 0, 0);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Dashboard Analytics", "Calculate KPI Statistics", ex.Message, ex);
            }
        }

        private static Product ReadProduct(SqliteDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = Convert.ToDecimal(reader.GetValue(2)),
                Quantity = reader.GetInt32(3),
                MinStock = reader.GetInt32(4)
            };
        }
    }
}

