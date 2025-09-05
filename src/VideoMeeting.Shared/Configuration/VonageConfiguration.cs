namespace VideoMeeting.Shared.Configuration;

public class VonageConfiguration
{
    public const string SectionName = "Vonage";
    
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string PrivateKeyPath { get; init; } = string.Empty;
}