using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Terpy.WEB;
using Terpy.WEB.Auth;
using Terpy.WEB.Repositories;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSweetAlert2();

builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://localhost:7039/") });
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationProviderJWT>();
builder.Services.AddScoped<AuthenticationStateProvider>(services => services.GetRequiredService<AuthenticationProviderJWT>());
builder.Services.AddScoped<ILoginService>(services => services.GetRequiredService<AuthenticationProviderJWT>());

await builder.Build().RunAsync();
