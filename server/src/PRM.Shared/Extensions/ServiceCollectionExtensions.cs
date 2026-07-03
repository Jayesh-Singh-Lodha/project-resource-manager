using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PRM.Application.Interfaces;
using PRM.Application.Services;
using PRM.Core.Interfaces;

using PRM.Infrastructure.Auth;
using PRM.Infrastructure.Data;
using PRM.Infrastructure.ExternalServices.Llm;
using PRM.Infrastructure.Repositories;

namespace PRM.Shared.Extensions;

/// <summary>
/// Extension methods for wiring up infrastructure services in the DI container.
/// Called from PRM.API/Program.cs to register all dependencies cleanly.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services: DbContext, repositories, auth services, JWT.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core — SQLite
        services.AddDbContext<PrmDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAllocationRepository, AllocationRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();

        // Auth services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAllocationService, AllocationService>();
        services.AddScoped<ITimesheetService, TimesheetService>();
        services.AddScoped<ISystemConfigService, SystemConfigService>();
        services.AddScoped<IAiAssistantService, AiAssistantService>();

        // LLM Providers
        services.AddHttpClient<GeminiProvider>();
        services.AddHttpClient<GroqProvider>();
        services.AddHttpClient<OllamaProvider>();
        services.AddScoped<ILlmProvider, GeminiProvider>();
        services.AddScoped<ILlmProvider, GroqProvider>();
        services.AddScoped<ILlmProvider, OllamaProvider>();

        // External Services
        services.Configure<PRM.Application.DTOs.Config.SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<IEmailNotificationService, PRM.Infrastructure.ExternalServices.EmailNotificationService>();

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer authentication from appsettings.json.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

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
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey))
            };
        });

        return services;
    }
}
