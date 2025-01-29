using Npgsql;
using System.Collections.ObjectModel;
using Task3.Models;

namespace Task3.Repositories;

public interface IProductRepository
{
    Task<long> CreateProductAsync(string name, decimal price, NpgsqlDataSource dataSource);

    Task<List<Product>> SearchProductsAsync(
        NpgsqlDataSource dataSource,
        int pageNumber,
        int pageSize,
        Collection<long>? productIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? nameSubstring = null);
}