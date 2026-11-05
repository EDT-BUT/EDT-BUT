using Edt.Domain.Timetables;

namespace Edt.Domain.Rooms;

public sealed record RoomAvailability(RoomEntity Room, CourseEntity? Current, CourseEntity? Next)
{
    public bool IsFree => Current is null;

    public TimeOnly? FreeUntil => IsFree ? Next?.Start : null;

    public TimeOnly? BusyUntil => Current?.End;

    public static RoomAvailability Compute(RoomEntity room, IEnumerable<CourseEntity> courses, DateOnly date, TimeOnly time)
    {
        var sameDay = courses.Where(c => c.Date == date).OrderBy(c => c.Start).ToList();

        var current = sameDay
            .Where(c => c.IsOngoingAt(date, time))
            .MaxBy(c => c.End);

        var next = sameDay.FirstOrDefault(c => c.Start > time);

        return new RoomAvailability(room, current, next);
    }
}
