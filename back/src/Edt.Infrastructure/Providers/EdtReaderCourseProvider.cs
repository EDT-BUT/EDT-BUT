using System.Text.Json;
using Edt.Application.Providers;
using Edt.Application.Providers.Dtos;
using Edt.Domain.Shared;
using Edt.Domain.Timetables;
using Edt.Infrastructure.HttpClients.EdtReader;
using Microsoft.Extensions.Logging;

namespace Edt.Infrastructure.Providers;

public sealed class EdtReaderCourseProvider(EdtReaderHttpClient client, ILogger<EdtReaderCourseProvider> logger)
    : ICourseProvider
{
    public async Task<Result<IReadOnlyList<CourseDto>>> GetByResourceIdAsync(int resourceId, CancellationToken cancellationToken)
    {
        try
        {
            var courses = await client.GetCoursesAsync(resourceId, cancellationToken);
            return Result.Success<IReadOnlyList<CourseDto>>(courses.Select(EdtReaderCourseMapper.ToDto).ToList());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or FormatException)
        {
            logger.LogWarning(ex, "EDTReader n'a pas renvoyé l'emploi du temps de la ressource {ResourceId}", resourceId);
            return Result.Failure<IReadOnlyList<CourseDto>>(TimetableErrors.Unavailable);
        }
    }
}
