using Edt.Domain.Timetables;

namespace Edt.Application.Services;

public interface ICalendarService
{
    string Build(string calendarName, IEnumerable<CourseEntity> courses, DateTimeOffset generatedAt);
}
