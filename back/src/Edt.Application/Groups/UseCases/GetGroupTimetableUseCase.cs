using Edt.Application.Groups.Mappers;
using Edt.Application.Groups.Responses;
using Edt.Application.Services;
using Edt.Application.Timetables;
using Edt.Domain.Campuses;
using Edt.Domain.Groups;
using Edt.Domain.Shared;

namespace Edt.Application.Groups.UseCases;

public sealed class GetGroupTimetableUseCase(
    IGroupRepository groupRepository,
    ICampusRepository campusRepository,
    ITimetableService timetableService,
    TimetableOptions options) : IGetGroupTimetableUseCase
{
    public async Task<Result<GetGroupTimetableResponse>> Execute(int id, CancellationToken cancellationToken)
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

        var campus = await campusRepository.GetByIdAsync(placement.Formation.CampusId, cancellationToken);
        return GroupMapper.ToGetGroupTimetableResponse(placement, campus?.Name ?? "", timetable.Value);
    }
}
