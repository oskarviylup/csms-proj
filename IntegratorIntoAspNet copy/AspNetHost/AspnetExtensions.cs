using IntegratorIntoAspNet.BackgroundService;
using IntegratorIntoAspNet.Controllers;
using IntegratorIntoAspNet.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegratorIntoAspNet.AspNetHost;

public class AspnetExtensions
{
    public void ConnectServicesAndConfigurationsToHost()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.AddServicesToHost();
        builder.AddServices();

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionFormattingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}