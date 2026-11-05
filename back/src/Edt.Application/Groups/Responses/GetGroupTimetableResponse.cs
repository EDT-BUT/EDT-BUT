using Edt.Application.Timetables.Responses;

namespace Edt.Application.Groups.Responses;

public sealed record GetGroupTimetableResponse(
    GroupDetailsResponse Group,
    IReadOnlyList<CourseResponse> Courses,
    DateTimeOffset FetchedAt,
    bool IsStale);
