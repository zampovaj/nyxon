namespace Nyxon.Core.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            //hash
            services.AddScoped<IHashService, Sha256HashService>();

            //crypto
            services.AddScoped<IRandomService, RandomService>();
            services.AddScoped<IAesCrypto, AesCrypto>();
            services.AddScoped<IArgon2Crypto, Argon2Crypto>();
            services.AddScoped<IEd25519Crypto, Ed25519Crypto>();
            services.AddScoped<IX25519Crypto, X25519Crypto>();

            //keys
            services.AddScoped<ISymmetricKeyService, SymmetricKeyService>();
            services.AddScoped<IKeyGenerationService, KeyGenerationService>();

            //services
            services.AddScoped<IVaultDecryptionService, MockDecryptionService>();

            return services;
        }
    }
}