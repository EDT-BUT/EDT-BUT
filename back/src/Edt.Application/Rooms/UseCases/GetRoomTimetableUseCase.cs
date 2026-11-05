using Edt.Application.Rooms.Mappers;
using Edt.Application.Rooms.Responses;
using Edt.Application.Services;
using Edt.Application.Timetables;
using Edt.Domain.Campuses;
using Edt.Domain.Rooms;
using Edt.Domain.Shared;

namespace Edt.Application.Rooms.UseCases;

public sealed class GetRoomTimetableUseCase(
    IRoomRepository roomRepository,
    ICampusRepository campusRepository,
    ITimetableService timetableService,
    TimetableOptions options) : IGetRoomTimetableUseCase
{
    public async Task<Result<GetRoomTimetableResponse>> Execute(int id, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(id, cancellationToken);
        if (room is null)
        {
            return RoomErrors.NotFound(id);
        }

        var timetable = await timetableService.GetAsync(id, options.RoomMaxAge, cancellationToken);
        if (timetable.IsFailure)
        {
            return timetable.Error;
        }

        var campus = await campusRepository.GetByIdAsync(room.CampusId, cancellationToken);
        return RoomMapper.ToGetRoomTimetableResponse(room, campus?.Name ?? "", timetable.Value);
    }
}
