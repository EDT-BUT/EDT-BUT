using Edt.Domain.Timetables;

namespace Edt.Application.Services;

public sealed record TimetableSnapshot(IReadOnlyList<CourseEntity> Courses, DateTimeOffset FetchedAt, bool IsStale);
