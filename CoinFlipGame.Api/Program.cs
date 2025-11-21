using CoinFlipGame.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Get connection string from configuration (optional for Azure Static Web Apps)
        var connectionString = context.Configuration.GetConnectionString("CoinFlipGameDb");

        // Only register DbContext if connection string is available
        // This allows the API to work in Azure Static Web Apps without a database
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<CoinFlipGameDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
    })
    .Build();

host.Run();
