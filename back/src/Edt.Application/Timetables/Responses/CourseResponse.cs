using Edt.Domain.Timetables;

namespace Edt.Application.Timetables.Responses;

public sealed record CourseResponse(
    string Date,
    string Start,
    string End,
    string? Code,
    string Name,
    CourseKind Kind,
    string Group,
    string Room,
    string Teacher);
