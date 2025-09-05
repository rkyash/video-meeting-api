using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoMeeting.Domain.Entities;
using VideoMeeting.Domain.Enums;

namespace VideoMeeting.Infrastructure.Persistence.Configurations;

public class MeetingParticipantConfiguration : IEntityTypeConfiguration<MeetingParticipant>
{
    public void Configure(EntityTypeBuilder<MeetingParticipant> builder)
    {
        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.GuestName)
            .HasMaxLength(100);

        builder.Property(mp => mp.GuestEmail)
            .HasMaxLength(255);

        builder.Property(mp => mp.ConnectionId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mp => mp.Role)
            .HasConversion<int>()
            .HasDefaultValue(ParticipantRole.Participant);

        builder.Property(mp => mp.IsMuted)
            .HasDefaultValue(false);

        builder.Property(mp => mp.IsVideoEnabled)
            .HasDefaultValue(true);

        builder.Property(mp => mp.IsScreenSharing)
            .HasDefaultValue(false);

        builder.Property(mp => mp.JoinedAt)
            .IsRequired();
    }
}