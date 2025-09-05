namespace VideoMeeting.Shared.Configuration;

public class DatabaseConfiguration
{
    public const string SectionName = "ConnectionStrings";
    
    public string DefaultConnection { get; init; } = string.Empty;
}