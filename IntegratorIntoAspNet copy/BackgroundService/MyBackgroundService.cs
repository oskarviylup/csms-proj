using ConfigServiceClient.DbConfigStructures;
using ConfigServiceClient.ManualConfigService;
using ConfigurationProviderService;
using ConfigurationProviderService.ConfigurationProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task3.Extensions;

namespace IntegratorIntoAspNet.BackgroundService;

public class MyBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly WebApplicationBuilder _builder;

    public MyBackgroundService(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await SomeInitialisationsBeforeAppStart(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"its {DateTimeOffset.UtcNow}");
        }
    }

    private async Task SomeInitialisationsBeforeAppStart(CancellationToken cancellationToken)
    {
        ServiceProvider provider = _builder.Services.BuildServiceProvider();
        var migrationsRunner = new RunMigrations();
        ServiceProvider migrationsProvider = migrationsRunner.CreateServices(_builder.Services, provider);
        await migrationsRunner.MigrateUpAsync(migrationsProvider);

        CustomConfigurationProvider customConfigurationProvider = provider.GetRequiredService<CustomConfigurationProvider>();
        IConfigurationServiceClient configuredClient = provider.GetRequiredService<IConfigurationServiceClient>();
        var configurationBuilder = new ConfigurationBuilder();

        var configService = new ConfigService(
            customConfigurationProvider,
            configuredClient,
            provider,
            new TimeSpan(0, 10, 0));
        IAsyncEnumerable<DbConfigItem> list = configService.FetchConfigurations(cancellationToken);
    }
}