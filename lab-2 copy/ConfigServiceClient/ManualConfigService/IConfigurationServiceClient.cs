using ConfigServiceClient.DbConfigStructures;
using System.Runtime.CompilerServices;

namespace ConfigServiceClient.ManualConfigService;

public interface IConfigurationServiceClient
{
    IAsyncEnumerable<DbConfigItem> GetConfigurationsList(
        IServiceProvider provider,
        CancellationToken cancellationToken);
}