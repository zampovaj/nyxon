using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this
using Microsoft.IdentityModel.Tokens; // Add this
using System.Text;
using Backend.Services.Messaging;

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

            // jwt
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                    };
                });

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connString));

            // valkey
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(config.GetConnectionString("Valkey")));

            services.AddScoped<IMessageCacheService, MessageCacheService>();
            services.AddScoped<IHashInterface, Sha256HashService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserVaultService, UserVaultService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();

            return services;
        }
    }
}