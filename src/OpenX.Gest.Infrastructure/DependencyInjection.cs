using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Infrastructure.Persistence;
using OpenX.Gest.Infrastructure.Persistence.Interceptors;
using OpenX.Gest.Infrastructure.Persistence.Repositories;
using OpenX.Gest.Infrastructure.Services;

namespace OpenX.Gest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Providers e Servizi
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISecoComplianceExportService, SecoComplianceExportService>();

        // Interceptor EF Core
        services.AddScoped<AuditLogInterceptor>();

        // DbContext SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=openx_gest.db";
        services.AddDbContext<OpenXGestDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditLogInterceptor>();
            options.UseSqlite(connectionString)
                   .AddInterceptors(interceptor);
        });

        // Repositories & UnitOfWork
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Autenticazione JWT
        var jwtSecret = configuration["Jwt:Secret"] ?? "OpenX_Enterprise_Super_Secret_Key_For_Swiss_TimeTracking_2026_Minimum_32_Bytes!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "OpenXGest";
        var jwtAudience = configuration["Jwt:Audience"] ?? "OpenXGestClient";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
