using System.Text;
using BuildingBlocks.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Application;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Services;

namespace Modules.Identity.Infrastructure;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            var useInMemory = configuration.GetValue<bool>("Identity:UseInMemoryDatabase");
            if (useInMemory)
            {
                options.UseInMemoryDatabase(configuration["Identity:InMemoryDatabaseName"] ?? "SmartCondoIdentity");
                return;
            }

            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' não configurada.");

            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityDbContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token");
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
                options.AddSigningKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SigningKey)));
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authOptions.Issuer,
                ValidAudience = authOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SigningKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IIdentityTokenService, IdentityTokenService>();

        return services;
    }
}
