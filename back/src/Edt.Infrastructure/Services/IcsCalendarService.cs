using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Edt.Application.Common;
using Edt.Application.Services;
using Edt.Domain.Timetables;

namespace Edt.Infrastructure.Services;

public sealed class IcsCalendarService : ICalendarService
{
    private const int MaxLineOctets = 75;

    public string Build(string calendarName, IEnumerable<CourseEntity> courses, DateTimeOffset generatedAt)
    {
        var ics = new StringBuilder();
        Append(ics, "BEGIN:VCALENDAR");
        Append(ics, "VERSION:2.0");
        Append(ics, "PRODID:-//EDT Unicaen//Prototype étudiant//FR");
        Append(ics, "CALSCALE:GREGORIAN");
        Append(ics, $"X-WR-CALNAME:{Escape(calendarName)}");
        Append(ics, "X-WR-TIMEZONE:Europe/Paris");
        Append(ics, "REFRESH-INTERVAL;VALUE=DURATION:PT1H");
        Append(ics, "X-PUBLISHED-TTL:PT1H");

        var stamp = Format(generatedAt.UtcDateTime);
        foreach (var course in courses)
        {
            Append(ics, "BEGIN:VEVENT");
            Append(ics, $"UID:{UidOf(course)}@edt-unicaen");
            Append(ics, $"DTSTAMP:{stamp}");
            Append(ics, $"DTSTART:{Format(ParisTime.ToUtc(course.Date, course.Start))}");
            Append(ics, $"DTEND:{Format(ParisTime.ToUtc(course.Date, course.End))}");
            Append(ics, $"SUMMARY:{Escape(SummaryOf(course))}");
            if (course.Room.Length > 0)
            {
                Append(ics, $"LOCATION:{Escape(course.Room)}");
            }
            Append(ics, $"DESCRIPTION:{Escape(DescriptionOf(course))}");
            Append(ics, "END:VEVENT");
        }

        Append(ics, "END:VCALENDAR");
        return ics.ToString();
    }

    private static string SummaryOf(CourseEntity course) => course.Kind switch
    {
        CourseKind.Lecture => $"{course.Title} (CM)",
        CourseKind.Tutorial => $"{course.Title} (TD)",
        CourseKind.Practical => $"{course.Title} (TP)",
        CourseKind.Exam => $"{course.Title} (examen)",
        _ => course.Title.ToString(),
    };

    private static string DescriptionOf(CourseEntity course) =>
        string.Join("\n", new[] { course.Teacher, course.Group }.Where(s => s.Length > 0));

    private static string UidOf(CourseEntity course)
    {
        var key = $"{course.Date:yyyyMMdd}|{course.Start:HHmm}|{course.Title}|{course.Group}";
        return Convert.ToHexStringLower(SHA1.HashData(Encoding.UTF8.GetBytes(key)));
    }

    private static string Format(DateTime utc) => utc.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

    private static string Escape(string text) => text
        .Replace("\\", "\\\\")
        .Replace(";", "\\;")
        .Replace(",", "\\,")
        .Replace("\r\n", "\\n")
        .Replace("\n", "\\n");

    private static void Append(StringBuilder ics, string line)
    {
        var octets = 0;
        foreach (var rune in line.EnumerateRunes())
        {
            var size = rune.Utf8SequenceLength;
            if (octets + size > MaxLineOctets)
            {
                ics.Append("\r\n ");
                octets = 1;
            }
            ics.Append(rune.ToString());
            octets += size;
        }
        ics.Append("\r\n");
    }
}
