using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CoinFlipGame.App;
using CoinFlipGame.App.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CoinFlipGame.App.Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CoinService>();
builder.Services.AddScoped<UnlockProgressService>(); // Scoped to allow consumption of scoped ILocalStorageService
builder.Services.AddScoped<UpdateService>(); // Service for app updates and cache management

await builder.Build().RunAsync();
