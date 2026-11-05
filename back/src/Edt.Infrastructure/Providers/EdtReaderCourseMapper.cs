using System.Globalization;
using System.Text.RegularExpressions;
using Edt.Application.Providers.Dtos;
using Edt.Domain.Timetables;
using Edt.Infrastructure.HttpClients.EdtReader;

namespace Edt.Infrastructure.Providers;

internal static partial class EdtReaderCourseMapper
{
    private static readonly TextInfo French = CultureInfo.GetCultureInfo("fr-FR").TextInfo;

    public static CourseDto ToDto(EdtReaderCourse raw) => new(
        new DateOnly(
            int.Parse(raw.Year, CultureInfo.InvariantCulture),
            int.Parse(raw.Month, CultureInfo.InvariantCulture),
            int.Parse(raw.Day, CultureInfo.InvariantCulture)),
        TimeOnly.ParseExact(raw.Start, "HH:mm", CultureInfo.InvariantCulture),
        TimeOnly.ParseExact(raw.End, "HH:mm", CultureInfo.InvariantCulture),
        raw.Title,
        ToKind(raw.Type),
        ToGroupLabel(raw.Group),
        raw.Room.Trim(),
        ToTeacherName(raw.Teacher));

    internal static CourseKind ToKind(string type) => type.Trim().ToUpperInvariant() switch
    {
        "CM" => CourseKind.Lecture,
        "TD" => CourseKind.Tutorial,
        "TP" => CourseKind.Practical,
        "CTRL" or "CTP" => CourseKind.Exam,
        _ => CourseKind.Other,
    };

    internal static string ToGroupLabel(string group) => PromotionPrefix().Replace(group.Trim(), "");

    internal static string ToTeacherName(string teacher)
    {
        var spaced = MissingSpace().Replace(teacher.Trim(), " ");
        if (spaced.Length == 0)
        {
            return "";
        }

        var words = spaced.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lastNames = words.Where(IsUpperCase).Select(w => French.ToTitleCase(w.ToLowerInvariant()));
        var firstNames = words.Where(w => !IsUpperCase(w));
        return string.Join(' ', firstNames.Concat(lastNames));
    }

    private static bool IsUpperCase(string word) =>
        word.Length > 1 && word.Any(char.IsLetter) && word.Where(char.IsLetter).All(char.IsUpper);

    [GeneratedRegex(@"^(?:(?:GRP|CMP)-[A-Z0-9]+|[A-Z]+\d)-(?=.)")]
    private static partial Regex PromotionPrefix();

    [GeneratedRegex(@"(?<=\p{Lu}{2})(?=\p{Lu}\p{Ll})")]
    private static partial Regex MissingSpace();
}
