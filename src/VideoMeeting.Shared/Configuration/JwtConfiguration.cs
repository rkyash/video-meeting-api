namespace VideoMeeting.Shared.Configuration;

public class JwtConfiguration
{
    public const string SectionName = "Jwt";
    
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromHours(1);
}