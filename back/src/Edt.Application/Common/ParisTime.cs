namespace Edt.Application.Common;

public static class ParisTime
{
    public static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");

    public static DateTime Now(TimeProvider time) =>
        TimeZoneInfo.ConvertTime(time.GetUtcNow(), Zone).DateTime;

    public static DateOnly Today(TimeProvider time) => DateOnly.FromDateTime(Now(time));

    public static DateTime ToUtc(DateOnly date, TimeOnly hour) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(hour), Zone);
}
