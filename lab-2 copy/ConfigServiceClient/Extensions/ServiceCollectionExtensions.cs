using ConfigServiceClient.ManualConfigService;
using ConfigServiceClient.RefitConfigService;
using CongifServiceClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace ConfigServiceClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddHttpConfigurationClient(this IServiceCollection collection)
    {
        collection.AddHttpClient<IConfigurationServiceClient, ManualConfigurationServiceClient>((provider, client) =>
        {
            ConfigurationServiceClientOptions options =
                provider.GetRequiredService<IOptions<ConfigurationServiceClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseAddress);
        });
    }

    public static void AddRefitConfigurationClient(this IServiceCollection collection)
    {
        collection.AddRefitClient<IRefitConfigurationServiceClient>()
            .ConfigureHttpClient(
                (provider, client) =>
                {
                    ConfigurationServiceClientOptions options =
                        provider.GetRequiredService<IOptions<ConfigurationServiceClientOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseAddress);
                });

        collection.AddTransient<IConfigurationServiceClient, RefitConfigurationServiceClient>();
    }

    public static void AddConfigurationClientOptions(this IServiceCollection collection)
    {
        collection.Configure<ConfigurationServiceClientOptions>(options =>
        {
            options.BaseAddress = "http://localhost:8080";
            options.PageSize = 5;
        });
    }
}