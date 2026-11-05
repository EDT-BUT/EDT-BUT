namespace Edt.Domain.Timetables;

public static class TimetableMerger
{
    public const int HistoryDays = 7;

    public static IReadOnlyList<CourseEntity> Merge(IReadOnlyList<CourseEntity>? previous, IReadOnlyList<CourseEntity> fresh, DateOnly today)
    {
        var oldest = today.AddDays(-HistoryDays);
        var past = (previous ?? []).Where(c => c.Date < today && c.Date >= oldest);

        return past
            .Concat(fresh)
            .Distinct()
            .OrderBy(c => c.Date)
            .ThenBy(c => c.Start)
            .ToList();
    }
}
