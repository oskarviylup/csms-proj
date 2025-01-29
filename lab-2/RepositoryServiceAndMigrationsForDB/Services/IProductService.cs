namespace Task3.Services;

public interface IProductService
{
    Task<long> CreateProduct(string name, decimal price);
}