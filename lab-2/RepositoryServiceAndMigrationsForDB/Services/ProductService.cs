using Npgsql;
using Task3.Repositories;

namespace Task3.Services;

public class ProductService(NpgsqlDataSource dataSource) : IProductService
{
    private readonly ProductRepository _productRepository = new();

    public async Task<long> CreateProduct(string name, decimal price)
    {
        return await _productRepository.CreateProductAsync(name, price, dataSource);
    }
}