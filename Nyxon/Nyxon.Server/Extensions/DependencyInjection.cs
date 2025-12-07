using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this
using Microsoft.IdentityModel.Tokens; // Add this
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Nyxon.Server.Hubs;
using Nyxon.Core.Interfaces;
using Nyxon.Server.Interfaces;
using Nyxon.Server.Services.Crypto;
using Nyxon.Server.Services.Vault;
using Nyxon.Server.Services.Messaging;
using Nyxon.Server.Services.Cache;
using Nyxon.Server.Data;
using Nyxon.Server.Services.Auth;

namespace Nyxon.Server.Extensions
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
            var valkeyConfig = config.GetConnectionString("Valkey");

            // fallback
            if (string.IsNullOrEmpty(valkeyConfig))
            {
                var vHost = Environment.GetEnvironmentVariable("VALKEY_HOST") ?? "localhost";
                var vPort = Environment.GetEnvironmentVariable("VALKEY_PORT") ?? "6379";

                valkeyConfig = $"{vHost}:{vPort},abortConnect=false";
            }

            // register valkey
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(valkeyConfig));
            // register IDistributedCache (This fixes the 500 error)
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = valkeyConfig;
                options.InstanceName = "Nyxon_"; // Optional prefix for keys
            });

            // signalr
            services.AddSignalR();

            // services
            services.AddScoped<IMessageCacheService, MessageCacheService>();
            services.AddScoped<IHashInterface, Sha256HashService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserVaultService, UserVaultService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IConversationVaultService, ConversationVaultService>();


            return services;
        }
    }
}