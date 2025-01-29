using ConfigurationProviderService.ConfigurationProvider;
using Microsoft.Extensions.Configuration;

namespace ConfigurationProviderService.ConfigSourse;

public class ConfigurationSource : IConfigurationSource
{
    private readonly CustomConfigurationProvider _provider;

    public ConfigurationSource(CustomConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return _provider;
    }
}