using ConfigServiceClient.DbConfigStructures;
using ConfigServiceClient.ManualConfigService;
using ConfigurationProviderService.ConfigurationProvider;

namespace ConfigurationProviderService;

public class ConfigService(CustomConfigurationProvider provider,
    IConfigurationServiceClient client,
    IServiceProvider serviceProvider,
    TimeSpan updateInterval) : IDisposable
{
    private readonly PeriodicTimer _timer = new(updateInterval);

    public async Task StartUpdatingConfigurations(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            IAsyncEnumerable<DbConfigItem> newConfigurations = FetchConfigurations(cancellationToken);

            if (provider.HasConfigurationChanged(newConfigurations))
            {
                provider.LoadConfigurations(newConfigurations);
            }
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    public IAsyncEnumerable<DbConfigItem> FetchConfigurations(CancellationToken cancellationToken)
    {
        IAsyncEnumerable<DbConfigItem> configItems = client.GetConfigurationsList(serviceProvider, cancellationToken);
        return configItems;
    }
}