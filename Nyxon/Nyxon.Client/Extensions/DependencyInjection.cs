using MudBlazor.Services;
using Nyxon.Client.Crypto;
using Nyxon.Client.Interfaces.Crypto;
using Nyxon.Client.Repositories;
using Nyxon.Core.Services.Hash;

namespace Nyxon.Client
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddClientServices(this IServiceCollection services, IWebAssemblyHostEnvironment environment)
        {
            //mudblazor
            services.AddMudServices();

            //csrf
            services.AddSingleton<CsrfTokenStore>();

            // http
            // register the handler first
            services.AddTransient<AntiforgeryHandler>();

            services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri(environment.BaseAddress);
            })
            .AddHttpMessageHandler<AntiforgeryHandler>();

            // configure the client with the handler
            services.AddHttpClient("Nyxon.ServerAPI", client =>
            {
                client.BaseAddress = new Uri(environment.BaseAddress);
            })
            .AddHttpMessageHandler<AntiforgeryHandler>();
            // this makes me able to just inject httpclient into razor files
            services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Nyxon.ServerAPI"));

            //crypto
            services.AddScoped<IArgon2Crypto, Argon2CryptoWasm>();
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<IHashService, Sha256HashService>();
            services.AddScoped<IX3DHCrypto, X3DHCrypto>();

            // repositories
            services.AddScoped<IVaultRepository, VaultRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IHandshakeRepository, HandshakeRepository>();

            // hub
            services.AddSingleton<IHubService, HubService>();

            // services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<LayoutService>();
            services.AddScoped<IUserVaultService, UserVaultService>();
            services.AddScoped<EncryptedUserVaultSessionService>();
            services.AddScoped<IInboxService, InboxService>();
            services.AddScoped<IUserListService, UserListService>();
            services.AddScoped<IUserListService, UserListService>();
            services.AddScoped<ClientOrchestratorService>();
            services.AddScoped<UserContext>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IActiveConversationService, ActiveConversationService>();
            services.AddScoped<IHandshakeService, HandshakeService>();

            //auth
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            services.AddScoped<AuthenticationStateProvider, HostAuthenticationStateProvider>();

            //cast for when we need the specific provider methods
            services.AddScoped(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

            // viewmodels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ChatViewModel>();

            return services;
        }
    }
}