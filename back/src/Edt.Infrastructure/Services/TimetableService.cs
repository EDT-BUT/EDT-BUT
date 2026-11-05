using System.Collections.Concurrent;
using Edt.Application.Common;
using Edt.Application.Providers;
using Edt.Application.Services;
using Edt.Application.Timetables.Mappers;
using Edt.Domain.Shared;
using Edt.Domain.Timetables;

namespace Edt.Infrastructure.Services;

public sealed class TimetableService(
    ICourseProvider courseProvider,
    ITimetableRepository timetableRepository,
    TimeProvider time) : ITimetableService
{
    private readonly ConcurrentDictionary<int, Lazy<Task<Result<TimetableSnapshot>>>> _refreshing = new();

    public async Task<Result<TimetableSnapshot>> GetAsync(int resourceId, TimeSpan maxAge, CancellationToken cancellationToken)
    {
        var stored = await timetableRepository.GetByResourceIdAsync(resourceId, cancellationToken);
        if (stored is not null && time.GetUtcNow() - stored.FetchedAt < maxAge)
        {
            return new TimetableSnapshot(stored.Courses, stored.FetchedAt, IsStale: false);
        }

        var refresh = _refreshing.GetOrAdd(
            resourceId,
            id => new Lazy<Task<Result<TimetableSnapshot>>>(() => RefreshAsync(id, stored)));
        try
        {
            return await refresh.Value.WaitAsync(cancellationToken);
        }
        finally
        {
            _refreshing.TryRemove(KeyValuePair.Create(resourceId, refresh));
        }
    }

    private async Task<Result<TimetableSnapshot>> RefreshAsync(int resourceId, TimetableEntity? stored)
    {
        var fresh = await courseProvider.GetByResourceIdAsync(resourceId, CancellationToken.None);
        if (fresh.IsFailure)
        {
            if (stored is null)
            {
                return fresh.Error;
            }

            return new TimetableSnapshot(stored.Courses, stored.FetchedAt, IsStale: true);
        }

        var courses = TimetableMerger.Merge(
            stored?.Courses,
            fresh.Value.Select(CourseMapper.ToEntity).ToList(),
            ParisTime.Today(time));
        var timetable = new TimetableEntity(resourceId, courses, time.GetUtcNow());

        await timetableRepository.SaveAsync(timetable, CancellationToken.None);
        return new TimetableSnapshot(timetable.Courses, timetable.FetchedAt, IsStale: false);
    }
}
