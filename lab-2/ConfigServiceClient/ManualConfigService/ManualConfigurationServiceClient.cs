using ConfigServiceClient.DbConfigStructures;
using CongifServiceClient.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ConfigServiceClient.ManualConfigService;

public class ManualConfigurationServiceClient(HttpClient httpClient) : IConfigurationServiceClient
{
    public IAsyncEnumerable<DbConfigItem> GetConfigurationsList(
        IServiceProvider provider,
        CancellationToken cancellationToken)
    {
        return ComposeConfigurationsAsync(provider, cancellationToken);
    }

    private async IAsyncEnumerable<IEnumerable<DbConfigItem>> GetConfigurationsPaginatedAsync(
        IServiceProvider provider, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? pageToken = null;
        var options = provider.GetRequiredService<IOptions<ConfigurationServiceClientOptions>>().Value;
        do
        {
            HttpResponseMessage response = await httpClient.GetAsync(
                $"configurations?pageSize={options.PageSize}&pageToken={pageToken}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            DbConfigResponse? configResponse = JsonSerializer.Deserialize<DbConfigResponse>(content);

            if (configResponse?.Items != null) yield return configResponse.Items;

            pageToken = configResponse?.PageToken;
        }
        while (!string.IsNullOrEmpty(pageToken) && !cancellationToken.IsCancellationRequested);
    }

    private async IAsyncEnumerable<DbConfigItem> ComposeConfigurationsAsync(
        IServiceProvider provider,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var configs in GetConfigurationsPaginatedAsync(provider, cancellationToken))
        {
            foreach (var configItem in configs)
                yield return configItem;
        }
    }
}