using Edt.Domain.Shared;

namespace Edt.Domain.Timetables;

public static class TimetableErrors
{
    public static readonly Error Unavailable =
        new("Timetable.Unavailable", "La source des emplois du temps ne répond pas pour le moment.", ErrorType.Unavailable);
}
