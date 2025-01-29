using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Task3.Migrations;

namespace Task3.Extensions;

public class RunMigrations
{
    public ServiceProvider CreateServices(IServiceCollection collection, ServiceProvider serviceProvider)
    {
        ServiceProvider provider = collection.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(serviceProvider.GetRequiredService<IOptions<DatabaseConfiguration>>()
                    .Value.ConnectionString)
                .WithMigrationsIn(typeof(InitialMigration).Assembly))
            .BuildServiceProvider(false);
        return provider;
    }

    public async Task MigrateUpAsync(ServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public async void MigrateDownAsync(ServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(202311140001);
    }
}