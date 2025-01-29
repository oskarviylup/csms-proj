using ConfigServiceClient.DbConfigStructures;
using Refit;

namespace ConfigServiceClient.RefitConfigService;

public interface IRefitConfigurationServiceClient
{
    [Get("/configurations")]
    Task<DbConfigResponse> GetConfigurationsList(
        IServiceProvider provider,
        CancellationToken cancellationToken);
}