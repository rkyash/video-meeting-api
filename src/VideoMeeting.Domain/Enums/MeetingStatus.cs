namespace VideoMeeting.Domain.Enums;

public enum MeetingStatus
{
    Scheduled = 0,
    Active = 1,
    Ended = 2,
    Cancelled = 3
}

public enum ParticipantRole
{
    Host = 0,
    Moderator = 1,
    Participant = 2,
    Guest = 3
}

public enum RecordingStatus
{
    Recording = 0,
    Processing = 1,
    Available = 2,
    Failed = 3,
    Deleted = 4,
    Completed=5
}