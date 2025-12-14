using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;

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

            //anti forgery
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "__Host-X-CSRF-TOKEN";
                options.Cookie.SameSite = SameSiteMode.Strict;
                // forces https
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // cookies
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "__Host-NyxonAuth";
                    //lax - industry standard
                    //allows google links but blocks malicious
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    //xss protection
                    options.Cookie.HttpOnly = true;

                    //aspnet redirects to login by default
                    //that would cause blazor to crash
                    //need to disbale redirect and return access denied instead
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    //same thing just return forbidden
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                });

            //authorization
            services.AddAuthorization();

            // db
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
            services.AddScoped<IHashService, Sha256HashService>();
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