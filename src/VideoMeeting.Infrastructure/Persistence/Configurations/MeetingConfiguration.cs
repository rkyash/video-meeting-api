using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Infrastructure.Persistence.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasIndex(m => m.SessionId).IsUnique();
        builder.HasIndex(m => m.RoomCode).IsUnique();

        builder.Property(m => m.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.SessionId)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(m => m.RoomCode)
            .HasMaxLength(10);

        builder.Property(m => m.Status)
            .HasConversion<int>()
            .HasDefaultValue(MeetingStatus.Scheduled);

        builder.Property(m => m.IsRecordingEnabled)
            .HasDefaultValue(true);

        builder.Property(m => m.IsScreenSharingEnabled)
            .HasDefaultValue(true);

        builder.Property(m => m.MaxParticipants)
            .HasDefaultValue(20);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.HasMany(m => m.Participants)
            .WithOne(p => p.Meeting)
            .HasForeignKey(p => p.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Recordings)
            .WithOne(r => r.Meeting)
            .HasForeignKey(r => r.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}