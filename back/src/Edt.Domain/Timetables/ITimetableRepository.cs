namespace Edt.Domain.Timetables;

public interface ITimetableRepository
{
    Task<TimetableEntity?> GetByResourceIdAsync(int resourceId, CancellationToken cancellationToken);

    Task SaveAsync(TimetableEntity timetable, CancellationToken cancellationToken);
}
