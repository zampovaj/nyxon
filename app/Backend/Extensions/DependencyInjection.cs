using Microsoft.Extensions.Configuration;

namespace Backend.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // postgres
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            // valkey
            services.AddSingleton<IConnectionMultiplexer>(sp => 
                ConnectionMultiplexer.Connect(config.GetConnectionString("Valkey")));
            
            services.AddScoped<IMessageCacheService, MessageCacheService>();
            services.AddScoped<IHashInterface, Sha256HashService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}