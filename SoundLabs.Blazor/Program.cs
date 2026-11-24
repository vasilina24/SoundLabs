using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SoundLabs.Blazor;
using SoundLabs.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001/";

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiUrl)
});

builder.Services.AddScoped<AuthStore>();
builder.Services.AddScoped<SoundLabsApi>();
builder.Services.AddScoped<CartState>();

await builder.Build().RunAsync();