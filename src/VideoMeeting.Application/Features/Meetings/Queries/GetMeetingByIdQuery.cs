using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Queries;

public record GetMeetingByIdQuery(int Id) : IRequest<MeetingResponseDto?>;

public record GetMeetingByRoomCodeQuery(string RoomCode) : IRequest<MeetingResponseDto?>;

public record GetUserMeetingsQuery(int UserId) : IRequest<List<MeetingListDto>>;

public record GetMeetingParticipantsQuery(int MeetingId) : IRequest<List<ParticipantDto>>;