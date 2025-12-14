using MudBlazor.Services;
using Nyxon.Core.Services.Hash;

namespace Nyxon.Client
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddClientServices(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
        {
            //mudblazor
            services.AddMudServices();

            // http
            // register the handler first
            services.AddTransient<AntiforgeryHandler>();

            // configure the client with the handler
            services.AddHttpClient("Nyxon.ServerAPI", client =>
            {
                client.BaseAddress = new Uri(environment.BaseAddress);
            })
            .AddHttpMessageHandler<AntiforgeryHandler>();
            // this makes me able to just inject httpclient into razor files
            services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Nyxon.ServerAPI"));

            // services
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IHashService, Sha256HashService>();

            //auth
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddScoped<AuthenticationStateProvider, HostAuthenticationStateProvider>();

            //cast for when we need the specific provider methods
            services.AddScoped(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

            // viewmodels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ChatViewModel>();

            return services;
        }
    }
}