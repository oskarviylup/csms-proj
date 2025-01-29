using ConfigurationProviderService.ConfigurationProvider;
using IntegratorIntoAspNet.BackgroundService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Task3.Extensions;
using Task3.Repositories;
using Task3.Services;

namespace IntegratorIntoAspNet.Extensions;

public static class ServicesExtension
{
    public static void AddServicesToHost(this WebApplicationBuilder builder)
    {
        builder.Host.ConfigureServices(x =>
        {
            x.AddScoped<IOrderRepository, OrderRepository>();
            x.AddScoped<IOrderItemRepository, OrderItemRepository>();
            x.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
            x.AddScoped<IProductRepository, ProductRepository>();
            x.AddScoped<IOrderService, OrderService>();
            x.AddScoped<IProductService, ProductService>();
            x.AddScoped<CustomConfigurationProvider>();
        });
    }

    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddServices();

        builder.Services.AddHostedService<MyBackgroundService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });
        builder.Services.AddControllers();
    }
}