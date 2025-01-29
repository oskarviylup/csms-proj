using ConfigServiceClient.DbConfigStructures;

namespace ConfigurationProviderService.ConfigurationProvider;

public class CustomConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
{
    public bool IsReloaded { get; set; }

    private IAsyncEnumerable<DbConfigItem>? CurrentConfigs { get; set; }

    public async void LoadConfigurations(IAsyncEnumerable<DbConfigItem> configurations)
    {
        await foreach (DbConfigItem item in configurations)
        {
            if (!Data.TryGetValue(item.Key, out string? value) || value != item.Value)
                Data[item.Key] = item.Value;
        }

        CurrentConfigs = configurations;
    }

    public bool HasConfigurationChanged(IAsyncEnumerable<DbConfigItem> newConfigurations)
    {
        return CurrentConfigs != null && !CurrentConfigs.Equals(newConfigurations);
    }
}