using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Infrastructure.Persistence.Configurations;

public class MeetingRecordingConfiguration : IEntityTypeConfiguration<MeetingRecording>
{
    public void Configure(EntityTypeBuilder<MeetingRecording> builder)
    {
        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.SessionId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mr => mr.RecordingId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mr => mr.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(mr => mr.FileUrl)
            .HasMaxLength(500);

        builder.Property(mr => mr.Status)
            .HasConversion<int>()
            .HasDefaultValue(RecordingStatus.Recording);

        builder.Property(mr => mr.StartedAt)
            .IsRequired();

        builder.Property(mr => mr.FileSizeBytes)
            .HasDefaultValue(0L);

        builder.Property(mr => mr.DurationSeconds)
            .HasDefaultValue(0);
    }
}