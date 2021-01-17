using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Refactored.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Refactored.Api.Repositories
{
    public class ProductOptionsRepository : BaseRepository
    {
        public ProductOptionsRepository(IConfiguration config)
            : base(config) 
        {
        }

        public async Task<List<ProductOption>> GetProductOptionsAsync(Guid productId)
        {
            var productOptions = new List<ProductOption>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM ProductOptions WHERE ProductId = '{productId}' COLLATE NOCASE";
                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        var productOption = ReadProductOption(reader);
                        productOptions.Add(productOption);
                    }
                }

                await connection.CloseAsync();
            }

            return productOptions;
        }

        public async Task<ProductOption> GetProductOptionAsync(Guid productId, Guid optionId)
        {
            ProductOption productOption = null;
            using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = 
                    "SELECT * FROM ProductOptions " +
                    $"WHERE ProductId = '{productId}' COLLATE NOCASE AND Id = '{optionId}' COLLATE NOCASE";
                using (var reader = command.ExecuteReader())
                {
                    if (await reader.ReadAsync())
                    {
                        productOption = ReadProductOption(reader);
                    }
                }
                await connection.CloseAsync();
            }

            return productOption;
        }

        public async Task<ProductOption> AddProductOptionAsync(NewProductOption productOption)
        {
            ProductOption savedProductOption = null;
            if (productOption != null && productOption.ProductId != Guid.Empty)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = 
                        "SELECT COUNT(*) FROM Products " +
                        $"WHERE Id = '{productOption.ProductId}' COLLATE NOCASE";
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    var productExists = count > 0;
                    if (!productExists)
                    {
                        throw new InvalidOperationException($"Product with Id '{productOption.ProductId}' does not exist.");
                    }

                    var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = 
                        "INSERT INTO ProductOptions (Id, ProductId, Name, Description) " +
                        $"VALUES ('{Guid.NewGuid()}', '{productOption.ProductId}', @Name, @Description)";
                    insertCommand.Parameters.AddWithValue("@Name", productOption.Name);
                    insertCommand.Parameters.AddWithValue("@Description", productOption.Description);

                    var readCommand = connection.CreateCommand();
                    readCommand.CommandText = $"SELECT * FROM Products WHERE Id = '{productOption.ProductId}' COLLATE NOCASE";

                    insertCommand.ExecuteNonQuery();
                    using (var reader = readCommand.ExecuteReader())
                    {
                        if (await reader.ReadAsync())
                        {
                            savedProductOption = ReadProductOption(reader);
                        }
                    }

                    await connection.CloseAsync();
                }
            }
            return savedProductOption;
        }

        public async Task UpdateOrAddProductOptionAsync(ProductOption productOption)
        {
            if (productOption != null && productOption.ProductId != Guid.Empty)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = 
                        "SELECT COUNT(*) FROM Products " +
                        $"WHERE Id = '{productOption.ProductId}' COLLATE NOCASE";
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    var productExists = count > 0;
                    if (!productExists)
                    {
                        await connection.CloseAsync();
                        throw new InvalidOperationException($"Product with Id '{productOption.ProductId}' does not exist.");
                    }

                    var shouldAdd = productOption.IsNew;
                    if (!shouldAdd)
                    {
                        command.CommandText =
                            "UPDATE ProductOptions SET Name = @Name, Description = @Description " +
                            $"WHERE Id = '{productOption.Id}' AND ProductId = '{productOption.ProductId}' COLLATE NOCASE";
                        command.Parameters.AddWithValue("@Name", productOption.Name);
                        command.Parameters.AddWithValue("@Description", productOption.Description);
                        var rowCount = command.ExecuteNonQuery();
                        shouldAdd = rowCount == 0;
                    }
                    if (shouldAdd)
                    {
                        command = connection.CreateCommand();
                        var productOptionId = productOption.Id != Guid.Empty ? productOption.Id : Guid.NewGuid();
                        command.CommandText =
                            "INSERT INTO ProductOptions (Id, ProductId, Name, Description) " +
                            $"VALUES ('{productOptionId}', '{productOption.ProductId}', @Name, @Description)";
                        command.Parameters.AddWithValue("@Name", productOption.Name);
                        command.Parameters.AddWithValue("@Description", productOption.Description);
                        command.ExecuteNonQuery();
                    }
                    await connection.CloseAsync();
                }
            }
        }

        public async Task DeleteProductOptionAsync(Guid productId, Guid productOptionId)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = 
                    "DELETE FROM ProductOptions " +
                    $"WHERE ProductId = '{productId}' COLLATE NOCASE AND Id = '{productOptionId}' COLLATE NOCASE";
                await connection.OpenAsync();
                command.ExecuteNonQuery();

                await connection.CloseAsync();
            }
        }

        private ProductOption ReadProductOption(SqliteDataReader reader)
        {
            var option = new ProductOption();
            option.Id = Guid.Parse(reader["Id"].ToString());
            option.ProductId = Guid.Parse(reader["ProductId"].ToString());
            option.Name = reader["Name"].ToString();
            option.Description = (DBNull.Value == reader["Description"]) ? null : reader["Description"].ToString();
            return option;
        }
    }
}
