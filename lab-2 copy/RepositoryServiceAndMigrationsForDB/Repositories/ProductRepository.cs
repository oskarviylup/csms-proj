using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;

namespace Task3.Repositories;

public class ProductRepository() : IProductRepository
{
    public async Task<long> CreateProductAsync(string name, decimal price, NpgsqlDataSource dataSource)
    {
        const string creatingQuery =
            "INSERT INTO products (product_name, product_price) VALUES (@name, @price) RETURNING product_id";
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        using var command = new NpgsqlCommand(creatingQuery, connection, transaction);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@price", price);

        long productId = (long)(await command.ExecuteScalarAsync() ?? throw new InvalidOperationException());
        await transaction.CommitAsync();
        return productId;
    }

    public async Task<List<Product>> SearchProductsAsync(
        NpgsqlDataSource dataSource,
        int pageNumber,
        int pageSize,
        Collection<long>? productIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? nameSubstring = null)
    {
        const string query = @"
            SELECT product_id, product_name, product_price 
            FROM products
            WHERE (cardinality(@productIds) = 0 OR product_id = ANY(@productIds))
              AND (@minPrice IS NULL OR product_price >= @minPrice)
              AND (@maxPrice IS NULL OR product_price <= @maxPrice)
              AND (@nameSubstring IS NULL OR product_name LIKE '%' || @nameSubstring || '%')
            ORDER BY product_id
            LIMIT @pageSize";

        var products = new List<Product>();

        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        using var command = new NpgsqlCommand(query, connection, transaction);

        command.Parameters.AddWithValue("@productIds", productIds ?? new Collection<long>());
        command.Parameters.AddWithValue("@minPrice", minPrice ?? throw new ArgumentNullException(nameof(minPrice)));
        command.Parameters.AddWithValue("@maxPrice", maxPrice ?? throw new ArgumentNullException(nameof(maxPrice)));
        command.Parameters.AddWithValue("@nameSubstring", nameSubstring ?? throw new ArgumentNullException(nameof(nameSubstring)));
        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);
        command.Parameters.AddWithValue("@pageSize", pageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
            });
        }

        await reader.CloseAsync();
        await transaction.CommitAsync();
        return products;
    }
}