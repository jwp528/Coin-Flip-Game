using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CoinFlipGame.App;
using CoinFlipGame.App.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CoinFlipGame.App.Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CoinService>();
builder.Services.AddSingleton<UnlockProgressService>(); // Singleton to persist across page lifecycle

await builder.Build().RunAsync();
