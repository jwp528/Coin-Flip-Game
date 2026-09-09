using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CoinFlipGame.App.Services;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CoinFlipGame.App.Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");
if (string.IsNullOrEmpty(apiBaseUrl))
{
    // In production (Azure Static Web Apps), use relative path
    apiBaseUrl = builder.HostEnvironment.BaseAddress;
}

var apiRoot = ResolveApiRoot(apiBaseUrl);

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    return new ApiVersionService(httpClient);
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AccountSessionStore>();
builder.Services.AddScoped<DeviceIdService>();
builder.Services.AddScoped<ExternalSignInService>();
builder.Services.AddScoped(sp => new CoinFlipApiClient(
    new HttpClient { BaseAddress = new Uri(apiRoot) },
    sp.GetRequiredService<AccountSessionStore>(),
    sp.GetRequiredService<DeviceIdService>()));
builder.Services.AddScoped<CoinService>();
builder.Services.AddScoped<UnlockProgressService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UpdateService>();

await builder.Build().RunAsync();

static string ResolveApiRoot(string apiBaseUrl)
{
    var trimmed = apiBaseUrl.TrimEnd('/');
    if (trimmed.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        return trimmed + "/";
    return trimmed + "/api/";
}
