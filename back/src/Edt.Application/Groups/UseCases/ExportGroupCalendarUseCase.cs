using Edt.Application.Services;
using Edt.Application.Timetables;
using Edt.Domain.Groups;
using Edt.Domain.Shared;

namespace Edt.Application.Groups.UseCases;

public sealed class ExportGroupCalendarUseCase(
    IGroupRepository groupRepository,
    ITimetableService timetableService,
    ICalendarService calendarService,
    TimetableOptions options,
    TimeProvider time) : IExportGroupCalendarUseCase
{
    public async Task<Result<string>> Execute(int id, CancellationToken cancellationToken)
    {
        var placement = await groupRepository.GetByIdAsync(id, cancellationToken);
        if (placement is null)
        {
            return GroupErrors.NotFound(id);
        }

        var timetable = await timetableService.GetAsync(id, options.GroupMaxAge, cancellationToken);
        if (timetable.IsFailure)
        {
            return timetable.Error;
        }

        var name = $"{placement.Formation.Name} {placement.Year.Year}A {placement.Group.Label}";
        return calendarService.Build(name, timetable.Value.Courses, time.GetUtcNow());
    }
}
