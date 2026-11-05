namespace Edt.Infrastructure.HttpClients.EdtReader;

public sealed class EdtReaderOptions
{
    public const string SectionName = "EdtReader";

    public string BaseUrl { get; init; } = "";

    public int MaxConcurrentRequests { get; init; } = 2;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);
}
