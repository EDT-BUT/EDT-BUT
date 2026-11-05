using Edt.Application.Timetables.Responses;

namespace Edt.Application.Rooms.Responses;

public sealed record GetRoomTimetableResponse(
    RoomResponse Room,
    string CampusName,
    IReadOnlyList<CourseResponse> Courses,
    DateTimeOffset FetchedAt,
    bool IsStale);
