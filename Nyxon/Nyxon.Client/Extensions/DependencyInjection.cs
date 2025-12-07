using MudBlazor.Services;
using Nyxon.Client.Interfaces;
using Nyxon.Client.Services.Hub;

namespace Nyxon.Client.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddClientServices(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
        {
            // mudblazor
            services.AddMudServices();

            //auth
            services.AddAuthorizationCore();
            // (We will implement CustomAuthStateProvider in Day 2 to handle JWTs)
            // services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            // http
            services.AddScoped(sp => new HttpClient
            {
                //BaseAddress = new Uri(environment.BaseAddress)
                BaseAddress = new Uri("http://localhost:8080") // dev only
            });

            // services
            services.AddScoped<IHubService, HubService>();
            
            return services;
        }
    }
}