using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualBasic;
using Nyxon.Server.Services.Crypto.Hash;
using Nyxon.Server.Services.Invites;
using Nyxon.Core.Interfaces.Crypto;
using Nyxon.Core.Crypto;
using Nyxon.Server.Services;
using Npgsql;

namespace Nyxon.Server.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            //cors
            if (isDevelopment)
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(corsPolicy =>
                    {
                        // TODO: allow any - fuck no
                        // "SetIsOriginAllowed(origin => true)" allows ANY origin (WSL, localhost, Caddy)
                        // This is much safer for dev than trying to guess the IP
                        corsPolicy.SetIsOriginAllowed(origin => true)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                    });
                });
            }

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

            //invite hash
            var serverSecret64 = Environment.GetEnvironmentVariable("NYXON_INVITE_HMAC_KEY");
            services.AddSingleton<IHasher>(new Hasher(serverSecret64));

            //anti forgery
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";

                if (isDevelopment)
                {
                    options.Cookie.Name = "X-CSRF-TOKEN";
                }
                else
                {
                    options.Cookie.Name = "__Host-X-CSRF-TOKEN";
                }
                options.Cookie.SameSite = SameSiteMode.Strict;
                //TODO: enforce https in production
                if (isDevelopment)
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                }
                else
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            });

            // cookies
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    if (isDevelopment)
                    {
                        options.Cookie.Name = "NyxonAuth"; // Simple name for Dev
                        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP
                        options.Cookie.SameSite = SameSiteMode.Lax; // Easier for Dev
                    }
                    else
                    {
                        //strict for production
                        options.Cookie.Name = "__Host-NyxonAuth";
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.Strict;
                    }

                    //sessionid validation
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        // get user id and session id form cookie
                        var userPrincipal = context.Principal;
                        var userId = userPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var cookieSessionId = userPrincipal?.FindFirst("SessionId")?.Value;

                        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(cookieSessionId))
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            return;
                        }

                        // fetch current session id from valkey
                        var sessionIdService = context.HttpContext.RequestServices.GetRequiredService<ISessionIdService>();
                        var activeSessionId = await sessionIdService.GetSessionIdAsync(userId);

                        // 3. Compare
                        if (activeSessionId != cookieSessionId)
                        {
                            // Mismatch! Someone else logged in. You are fired.
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        }
                    };
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
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanCreateInvites", policy =>
                    policy.RequireClaim("CanCreateInvites", "True"));
            });

            // db
            // 1. Create the DataSourceBuilder using your existing connString
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);

            // 2. 🔥 ENABLE DYNAMIC JSON (Fixes the 8.0 breaking change)
            dataSourceBuilder.EnableDynamicJson();

            // 3. Build the DataSource
            var dataSource = dataSourceBuilder.Build();

            // 4. Register DbContext using the data source instead of just the string
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(dataSource));

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
            services.AddScoped<IUserVaultService, UserVaultService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IConversationVaultService, ConversationVaultService>();
            services.AddScoped<ISessionIdService, SessionIdService>();
            services.AddScoped<IInviteCodeService, InviteCodeService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IHandshakeService, HandshakeService>();
            services.AddScoped<IPrekeyService, PrekeyService>();
            services.AddScoped<ISnapshotService, SnapshotService>();

            return services;
        }
    }
}