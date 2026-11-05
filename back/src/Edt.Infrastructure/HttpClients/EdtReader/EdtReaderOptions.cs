namespace Edt.Infrastructure.HttpClients.EdtReader;

public sealed class EdtReaderOptions
{
    public const string SectionName = "EdtReader";

    public string BaseUrl { get; init; } = "https://zimmermanna.users.greyc.fr/edt/php/EDTReader.php";

    public int MaxConcurrentRequests { get; init; } = 2;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);
}
