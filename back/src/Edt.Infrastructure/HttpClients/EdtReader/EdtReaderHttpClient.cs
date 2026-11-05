using System.Text.Json;

namespace Edt.Infrastructure.HttpClients.EdtReader;

public sealed class EdtReaderHttpClient(IHttpClientFactory httpClientFactory, EdtReaderOptions options) : IDisposable
{
    public const string Name = "EdtReader";

    private readonly SemaphoreSlim _throttle = new(options.MaxConcurrentRequests);

    public async Task<IReadOnlyList<EdtReaderCourse>> GetCoursesAsync(int resourceId, CancellationToken cancellationToken)
    {
        await _throttle.WaitAsync(cancellationToken);
        try
        {
            var http = httpClientFactory.CreateClient(Name);
            using var response = await http.GetAsync($"?ressource={resourceId}&format=week", cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<List<EdtReaderCourse>>(body, cancellationToken: cancellationToken) ?? [];
        }
        finally
        {
            _throttle.Release();
        }
    }

    public void Dispose() => _throttle.Dispose();
}
