using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CoinFlipGame.App;
using CoinFlipGame.App.Services;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CoinFlipGame.App.Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure API HttpClient
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");
if (string.IsNullOrEmpty(apiBaseUrl))
{
    // In production (Azure Static Web Apps), use relative path
    apiBaseUrl = builder.HostEnvironment.BaseAddress;
}

builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    return new ApiVersionService(httpClient);
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CoinService>();
builder.Services.AddScoped<UnlockProgressService>(); // Scoped to allow consumption of scoped ILocalStorageService
builder.Services.AddScoped<UpdateService>(); // Service for app updates and cache management

await builder.Build().RunAsync();
