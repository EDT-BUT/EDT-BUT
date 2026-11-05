namespace Edt.Domain.Timetables;

public sealed record CourseEntity(
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    CourseTitle Title,
    CourseKind Kind,
    string Group,
    string Room,
    string Teacher)
{
    public bool IsOngoingAt(DateOnly date, TimeOnly time) =>
        Date == date && Start <= time && time < End;
}
