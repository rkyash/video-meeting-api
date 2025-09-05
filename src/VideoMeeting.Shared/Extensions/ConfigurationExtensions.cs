using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VideoMeeting.Shared.Configuration;

namespace VideoMeeting.Shared.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.SectionName));
        services.Configure<VonageConfiguration>(configuration.GetSection(VonageConfiguration.SectionName));
        services.Configure<DatabaseConfiguration>(configuration.GetSection(DatabaseConfiguration.SectionName));
        
        services.AddSingleton<JwtConfiguration>(provider => 
            provider.GetRequiredService<IOptions<JwtConfiguration>>().Value);
        services.AddSingleton<VonageConfiguration>(provider => 
            provider.GetRequiredService<IOptions<VonageConfiguration>>().Value);
        services.AddSingleton<DatabaseConfiguration>(provider => 
            provider.GetRequiredService<IOptions<DatabaseConfiguration>>().Value);
        
        services.AddSingleton<AppConfiguration>(provider => new AppConfiguration
        {
            Jwt = provider.GetRequiredService<JwtConfiguration>(),
            Vonage = provider.GetRequiredService<VonageConfiguration>(),
            ConnectionStrings = provider.GetRequiredService<DatabaseConfiguration>(),
            AllowedHosts = configuration["AllowedHosts"] ?? "*"
        });
        
        return services;
    }
    
    public static T GetConfiguration<T>(this IConfiguration configuration, string sectionName) where T : class, new()
    {
        return configuration.GetSection(sectionName).Get<T>() ?? new T();
    }
}