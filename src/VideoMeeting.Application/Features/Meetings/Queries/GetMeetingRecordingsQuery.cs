using MediatR;
using VideoMeeting.Application.Features.Meetings.DTOs;

namespace VideoMeeting.Application.Features.Meetings.Queries;

public record GetMeetingRecordingsQuery(string RoomCode) : IRequest<List<RecordingDto>>;