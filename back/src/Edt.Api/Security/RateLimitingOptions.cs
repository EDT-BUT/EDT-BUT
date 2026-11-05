namespace Edt.Api.Security;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int ApiPermitsPerMinute { get; init; } = 300;

    public int RoomsStatusPermitsPerMinute { get; init; } = 20;
}
