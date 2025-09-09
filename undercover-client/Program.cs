using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using undercover_client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services (word pairs, local game service)
builder.Services.AddScoped<RoamingRoutes.Client.Services.IWordPairClientService, RoamingRoutes.Client.Services.WordPairClientService>();
builder.Services.AddScoped<RoamingRoutes.Client.Services.ILocalGameService, RoamingRoutes.Client.Services.LocalGameService>();

await builder.Build().RunAsync();
