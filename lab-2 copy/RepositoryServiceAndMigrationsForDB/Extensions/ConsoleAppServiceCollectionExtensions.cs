using ConfigServiceClient.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Task3.Repositories;
using Task3.Services;

namespace Task3.Extensions;

public static class ConsoleAppServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddScoped<IOrderRepository, OrderRepository>();
        collection.AddScoped<IOrderItemRepository, OrderItemRepository>();
        collection.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
        collection.AddScoped<IProductRepository, ProductRepository>();
        collection.AddScoped<IOrderService, OrderService>();
        collection.AddScoped<IProductService, ProductService>();
        collection.AddScoped<CustomConfigurationProvider>();
        collection.AddConfigurationClientOptions();
        collection.AddHttpConfigurationClient();
    }
}