using System.Text.RegularExpressions;

namespace Edt.Domain.Timetables;

public sealed partial record CourseTitle(string? Code, string Name)
{
    public static CourseTitle Parse(string raw)
    {
        var text = SessionSuffix().Replace(raw.Trim(), "");
        var match = ModuleCode().Match(text);

        return match.Success
            ? new CourseTitle(match.Groups["code"].Value, match.Groups["name"].Value.Trim())
            : new CourseTitle(null, text);
    }

    public override string ToString() => Code is null ? Name : $"{Code} {Name}";

    [GeneratedRegex(@"\s+[A-Za-z]{2,4}_s?\d{1,2}$")]
    private static partial Regex SessionSuffix();

    [GeneratedRegex(@"^(?<code>(?:R|S|SA[EÉ])\s?\d+\.\d+[A-Za-z]?)\s*:?\s*(?<name>.+)$")]
    private static partial Regex ModuleCode();
}
