using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Backend.Data;

namespace Backend.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {

            var connString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connString))
            {
                var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
                var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
                var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB");
                var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
                var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
                connString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
            }

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connString));

            // valkey
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(config.GetConnectionString("Valkey")));

            services.AddScoped<IMessageCacheService, MessageCacheService>();
            services.AddScoped<IHashInterface, Sha256HashService>();
            services.AddScoped<IRegistrationService, RegistrationService>();

            return services;
        }
    }
}