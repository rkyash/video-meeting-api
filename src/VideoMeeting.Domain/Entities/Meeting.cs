using VideoMeeting.Domain.Common;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Domain.Entities;

public class Meeting : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? RoomCode { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsRecordingEnabled { get; set; } = true;
    public bool IsScreenSharingEnabled { get; set; } = true;
    public int MaxParticipants { get; set; } = 15;
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

    public virtual User? CreatedBy { get; set; }
    public virtual ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public virtual ICollection<MeetingRecording> Recordings { get; set; } = new List<MeetingRecording>();

    public bool IsActive => Status == MeetingStatus.Active;
    public bool CanBeStarted => Status == MeetingStatus.Scheduled;
    public bool CanBeEnded => Status == MeetingStatus.Active;
    public int ActiveParticipantCount => Participants.Count(p => p.LeftAt == null);
}