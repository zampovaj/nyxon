using MudBlazor.Services;
using Nyxon.Client.Crypto;
using Nyxon.Client.Interfaces.Crypto;
using Nyxon.Client.Repositories;
using Nyxon.Core.Services.Hash;
using Nyxon.Core.Services.Vault;

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

            //crypto
            services.AddScoped<IVaultDecryptionService, MockDecryptionService>();
            services.AddScoped<IArgon2Crypto, Argon2CryptoWasm>();
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<IHashService, Sha256HashService>();

            // services
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<LayoutService>();
            services.AddScoped<IUserVaultService, UserVaultService>();
            services.AddScoped<EncryptedUserVaultSessionService>();
            services.AddScoped<IVaultRepository, VaultRepository>();

            //auth
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddScoped<AuthenticationStateProvider, HostAuthenticationStateProvider>();

            //cast for when we need the specific provider methods
            services.AddScoped(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

            // viewmodels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();

            return services;
        }
    }
}