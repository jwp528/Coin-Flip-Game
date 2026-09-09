using Azure.Data.Tables;
using CoinFlipGame.Api.Persistence;
using CoinFlipGame.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddMemoryCache();
        services.AddSingleton<CoinStorageService>();

        services.AddSingleton(_ =>
        {
            var connectionString = configuration["TablesStorageConnectionString"]
                ?? configuration["AzureWebJobsStorage"]
                ?? "UseDevelopmentStorage=true";
            return new TableServiceClient(connectionString);
        });

        services.AddSingleton<TableStorageService>();
        services.AddSingleton<PlayerAccountService>();
        services.AddSingleton<PlayerProgressService>();
        services.AddHttpClient(nameof(ExternalAuthService), client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddSingleton<ExternalAuthService>();
    })
    .Build();

try
{
    var tables = host.Services.GetRequiredService<TableStorageService>();
    await tables.EnsureTablesExistAsync();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    logger.LogError(ex, "Failed to ensure Azure Table Storage tables exist during startup; continuing without failing the host.");
}

host.Run();
