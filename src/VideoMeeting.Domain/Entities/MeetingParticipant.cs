using VideoMeeting.Domain.Common;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Domain.Entities;

public class MeetingParticipant : BaseEntity
{
    public long MeetingId { get; set; }
    public long? UserId { get; set; }
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public ParticipantRole Role { get; set; } = ParticipantRole.Participant;
    public bool IsMuted { get; set; } = false;
    public bool IsVideoEnabled { get; set; } = true;
    public bool IsScreenSharing { get; set; } = false;

    public virtual Meeting Meeting { get; set; } = null!;
    public virtual User? User { get; set; }

    public bool IsGuest => UserId == null;
    public bool IsActive => LeftAt == null;
    public string DisplayName => User?.FullName ?? GuestName ?? "Unknown User";
    public TimeSpan? SessionDuration => LeftAt?.Subtract(JoinedAt) ?? DateTime.UtcNow.Subtract(JoinedAt);
}