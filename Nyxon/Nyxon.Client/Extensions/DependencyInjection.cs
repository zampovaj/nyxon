using MudBlazor.Services;
using Nyxon.Client.Interfaces;
using Nyxon.Client.Services.Hub;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Nyxon.Client.Services;
using Nyxon.Client.State;

namespace Nyxon.Client
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddClientServices(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
        {
            services.AddMudServices();

            // basic setup
            services.AddScoped(sp => new HttpClient
            {
                //BaseAddress = new Uri(environment.BaseAddress)
                BaseAddress = new Uri("http://localhost:8080") // DEV ONLY
            });

            // state
            services.AddSingleton<AppState>();
            services.AddScoped<LayoutService>(); // ONLY FOR NOW

            // services
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // viewmodel
            // services.AddTransient<LoginViewModel>();
            // services.AddTransient<ChatViewModel>();

            return services;
        }
    }
}