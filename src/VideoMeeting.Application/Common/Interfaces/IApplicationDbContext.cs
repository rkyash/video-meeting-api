using Microsoft.EntityFrameworkCore;
using VideoMeeting.Domain.Entities;

namespace VideoMeeting.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Meeting> Meetings { get; }
    DbSet<MeetingParticipant> MeetingParticipants { get; }
    DbSet<MeetingRecording> MeetingRecordings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}