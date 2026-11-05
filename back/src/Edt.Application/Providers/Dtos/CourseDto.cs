using Edt.Domain.Timetables;

namespace Edt.Application.Providers.Dtos;

public sealed record CourseDto(
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string Title,
    CourseKind Kind,
    string Group,
    string Room,
    string Teacher);
