using CoinFlipGame.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Get connection string from configuration
        var connectionString = context.Configuration.GetConnectionString("CoinFlipGameDb")
            ?? throw new InvalidOperationException("Connection string 'CoinFlipGameDb' not found.");

        // Register DbContext
        services.AddDbContext<CoinFlipGameDbContext>(options =>
            options.UseSqlServer(connectionString));
    })
    .Build();

host.Run();
