namespace Edt.Domain.Timetables;

public sealed record TimetableEntity(int ResourceId, IReadOnlyList<CourseEntity> Courses, DateTimeOffset FetchedAt);
