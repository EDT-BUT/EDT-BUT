using System.Collections.Concurrent;
using Edt.Domain.Timetables;

namespace Edt.Infrastructure.Repositories;

public sealed class InMemoryTimetableRepository : ITimetableRepository
{
    private readonly ConcurrentDictionary<int, TimetableEntity> _timetables = new();

    public Task<TimetableEntity?> GetByResourceIdAsync(int resourceId, CancellationToken cancellationToken) =>
        Task.FromResult(_timetables.GetValueOrDefault(resourceId));

    public Task SaveAsync(TimetableEntity timetable, CancellationToken cancellationToken)
    {
        _timetables[timetable.ResourceId] = timetable;
        return Task.CompletedTask;
    }
}
