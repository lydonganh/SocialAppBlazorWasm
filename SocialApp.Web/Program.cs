using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SocialApp.Web;
using SocialApp.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://m3qcc6b6-7175.asse.devtunnels.ms")
});
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
