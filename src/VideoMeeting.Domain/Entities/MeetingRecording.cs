using VideoMeeting.Domain.Common;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Domain.Entities;

public class MeetingRecording : BaseEntity
{
    public long MeetingId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string RecordingId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public RecordingStatus Status { get; set; } = RecordingStatus.Recording;

    public virtual Meeting Meeting { get; set; } = null!;

    public bool IsCompleted => Status == RecordingStatus.Available;
    public bool IsProcessing => Status == RecordingStatus.Processing;
    public bool HasFailed => Status == RecordingStatus.Failed;
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
    public string FormattedFileSize => FormatFileSize(FileSizeBytes);

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1} {suffixes[counter]}";
    }
}