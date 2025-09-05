namespace VideoMeeting.Shared.Configuration;

public class AppConfiguration
{
    public JwtConfiguration Jwt { get; init; } = new();
    public VonageConfiguration Vonage { get; init; } = new();
    public DatabaseConfiguration ConnectionStrings { get; init; } = new();
    public string AllowedHosts { get; init; } = "*";
}