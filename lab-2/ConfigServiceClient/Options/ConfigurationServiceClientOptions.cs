namespace CongifServiceClient.Options;

public class ConfigurationServiceClientOptions
{
    public string BaseAddress { get; set; } = string.Empty;

    public int PageSize { get; set; } = 10;
}