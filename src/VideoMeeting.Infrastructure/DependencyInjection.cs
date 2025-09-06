using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoMeeting.Application.Common.Interfaces;
using VideoMeeting.Infrastructure.Persistence;
using VideoMeeting.Infrastructure.Services;
using VideoMeeting.Shared.Configuration;

namespace VideoMeeting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get database configuration from shared config
        var dbConfig = configuration.GetSection(DatabaseConfiguration.SectionName).Get<DatabaseConfiguration>()
                      ?? throw new InvalidOperationException("Database configuration is missing");

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                dbConfig.DefaultConnection,
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Vonage Video Services
        services.AddScoped<Vonage.Video.Authentication.IVideoTokenGenerator, Vonage.Video.Authentication.VideoTokenGenerator>();
        
        // Services - these will use injected configuration classes
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVonageService, VonageService>();

        return services;
    }
}