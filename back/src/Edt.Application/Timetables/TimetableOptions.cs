namespace Edt.Application.Timetables;

public sealed class TimetableOptions
{
    public TimeSpan GroupMaxAge { get; init; } = TimeSpan.FromMinutes(30);

    public TimeSpan RoomMaxAge { get; init; } = TimeSpan.FromHours(3);
}
