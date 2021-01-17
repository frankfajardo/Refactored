using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Refactored.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Refactored.Api.Repositories
{
    public class ProductsRepository : BaseRepository
    {
        public ProductsRepository(IConfiguration config)
            : base(config)
        {
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var products = new List<Product>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Products";
                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        var product = ReadProduct(reader);
                        products.Add(product);
                    }
                }
                await connection.CloseAsync();
            }
            return products;
        }

        public async Task<Product> GetProductAsync(Guid id)
        {
            Product product = null;
            if (id != Guid.Empty)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM Products WHERE Id = '{id}' COLLATE NOCASE";
                    using (var reader = command.ExecuteReader())
                    {
                        if (await reader.ReadAsync())
                        {
                            product = ReadProduct(reader);
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            return product;
        }

        public async Task<Product> AddProductAsync(NewProduct product)
        {
            Product savedProduct = null;
            if (product != null)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    var insertCommand = connection.CreateCommand();
                    var newProductId = Guid.NewGuid();
                    insertCommand.CommandText = $"INSERT INTO Products (Id, Name, Description, Price, DeliveryPrice) VALUES ('{newProductId}', @Name, @Description, @Price, @DeliveryPrice)";
                    insertCommand.Parameters.AddWithValue("@Name", product.Name);
                    insertCommand.Parameters.AddWithValue("@Description", product.Description);
                    insertCommand.Parameters.AddWithValue("@Price", product.Price);
                    insertCommand.Parameters.AddWithValue("@DeliveryPrice", product.DeliveryPrice);

                    var readCommand = connection.CreateCommand();
                    readCommand.CommandText = $"SELECT * FROM Products WHERE Id = '{newProductId}' COLLATE NOCASE";

                    await connection.OpenAsync();
                    insertCommand.ExecuteNonQuery();
                    using (var reader = readCommand.ExecuteReader())
                    {
                        if (await reader.ReadAsync())
                        {
                            savedProduct = ReadProduct(reader);
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            return savedProduct;
        }

        public async Task UpdateOrAddProductAsync(Product product)
        {
            if (product != null)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    var shouldAdd = product.IsNew;
                    if (!shouldAdd)
                    {
                        command.CommandText =
                            "UPDATE Products " +
                            "SET Name = @Name, Description = @Description, Price = @Price, DeliveryPrice = @DeliveryPrice " +
                            $"WHERE Id = '{product.Id}' COLLATE NOCASE";
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@DeliveryPrice", product.DeliveryPrice);
                        var rowCount = command.ExecuteNonQuery();
                        shouldAdd = rowCount == 0;
                    }
                    if (shouldAdd)
                    {
                        command = connection.CreateCommand();
                        var productId = product.Id != Guid.Empty ? product.Id : Guid.NewGuid();
                        command.CommandText = 
                            "INSERT INTO Products (Id, Name, Description, Price, DeliveryPrice) " +
                            $"VALUES ('{productId}', @Name, @Description, @Price, @DeliveryPrice)";
                        command.Parameters.AddWithValue("@Name", product.Name);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@DeliveryPrice", product.DeliveryPrice);
                        command.ExecuteNonQuery();
                    }
                    await connection.CloseAsync();
                }
            }
        }

        public async Task DeleteProductAsync(Guid id)
        {
            if (id != Guid.Empty)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction(deferred: true))
                    {
                        var deleteProductOptions = connection.CreateCommand();
                        deleteProductOptions.CommandText = $"DELETE FROM ProductOptions WHERE ProductId = '{id}' COLLATE NOCASE";
                        deleteProductOptions.ExecuteNonQuery();

                        var deleteProduct = connection.CreateCommand();
                        deleteProduct.CommandText = $"DELETE FROM Products WHERE Id = '{id}' COLLATE NOCASE";
                        deleteProduct.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    await connection.CloseAsync();
                }

            }
        }

        private Product ReadProduct(SqliteDataReader reader)
        {
            var product = new Product();
            product.Id = Guid.Parse(reader["Id"].ToString());
            product.Name = reader["Name"].ToString();
            product.Description = (DBNull.Value == reader["Description"]) ? null : reader["Description"].ToString();
            product.Price = decimal.Parse(reader["Price"].ToString());
            product.DeliveryPrice = decimal.Parse(reader["DeliveryPrice"].ToString());
            return product;
        }
    }
}
