using ConfigServiceClient.ManualConfigService;
using ConfigurationProviderService.ConfigSourse;
using ConfigurationProviderService.ConfigurationProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationProviderService.Extensions;

public static class ConfigurationExtensions
{
    public static async Task<IConfiguration> AddCustomConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        CustomConfigurationProvider customConfigurationProvider = serviceProvider.GetRequiredService<CustomConfigurationProvider>();
        IConfigurationServiceClient configuredClient = serviceProvider.GetRequiredService<IConfigurationServiceClient>();
        configurationBuilder.Add(new ConfigurationSource(customConfigurationProvider));

        var configService = new ConfigService(
            customConfigurationProvider,
            configuredClient,
            serviceProvider,
            new TimeSpan(0, 10, 0));

        await configService.StartUpdatingConfigurations(cancellationToken);
        IConfiguration configuration = configurationBuilder.Build();
        return configuration;
    }

    public static void AddConfigurationProvider(IServiceCollection collection)
    {
        collection.AddScoped<CustomConfigurationProvider>();
    }
}