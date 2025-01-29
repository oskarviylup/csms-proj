using ConfigServiceClient.DbConfigStructures;
using ConfigServiceClient.ManualConfigService;
using System.Runtime.CompilerServices;

namespace ConfigServiceClient.RefitConfigService;

public class RefitConfigurationServiceClient
    (IRefitConfigurationServiceClient client) : IConfigurationServiceClient
{
    public async IAsyncEnumerable<DbConfigItem> GetConfigurationsList(
        IServiceProvider provider,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DbConfigResponse response = await client.GetConfigurationsList(provider, cancellationToken);

        foreach (var item in response.Items)
            yield return item;
    }
}