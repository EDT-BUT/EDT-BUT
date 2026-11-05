using System.Globalization;
using Edt.Application.Providers.Dtos;
using Edt.Application.Timetables.Responses;
using Edt.Domain.Timetables;

namespace Edt.Application.Timetables.Mappers;

public static class CourseMapper
{
    public static CourseEntity ToEntity(CourseDto dto) => new(
        dto.Date,
        dto.Start,
        dto.End,
        CourseTitle.Parse(dto.Title),
        dto.Kind,
        dto.Group,
        dto.Room,
        dto.Teacher);

    public static CourseResponse ToResponse(CourseEntity course) => new(
        course.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        course.Start.ToString("HH:mm", CultureInfo.InvariantCulture),
        course.End.ToString("HH:mm", CultureInfo.InvariantCulture),
        course.Title.Code,
        course.Title.Name,
        course.Kind,
        course.Group,
        course.Room,
        course.Teacher);

    public static IReadOnlyList<CourseResponse> ToResponses(IEnumerable<CourseEntity> courses) =>
        courses.Select(ToResponse).ToList();
}
