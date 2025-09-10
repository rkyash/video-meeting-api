using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Queries;

public record GetMeetingByIdQuery(string RoomCode) : IRequest<MeetingResponseDto?>;

public record GetMeetingByRoomCodeQuery(string RoomCode) : IRequest<MeetingResponseDto?>;

public record GetMeetingParticipantsQuery(string RoomCode) : IRequest<List<ParticipantDto>>;