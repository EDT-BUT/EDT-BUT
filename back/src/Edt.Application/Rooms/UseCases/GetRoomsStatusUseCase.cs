using Edt.Application.Common;
using Edt.Application.Rooms.Mappers;
using Edt.Application.Rooms.Models;
using Edt.Application.Rooms.Responses;
using Edt.Application.Services;
using Edt.Application.Timetables;
using Edt.Domain.Campuses;
using Edt.Domain.Rooms;
using Edt.Domain.Shared;

namespace Edt.Application.Rooms.UseCases;

public sealed class GetRoomsStatusUseCase(
    ICampusRepository campusRepository,
    IRoomRepository roomRepository,
    ITimetableService timetableService,
    TimetableOptions options,
    TimeProvider time) : IGetRoomsStatusUseCase
{
    public async Task<Result<GetRoomsStatusResponse>> Execute(GetRoomsStatusModel model, CancellationToken cancellationToken)
    {
        if (model.CampusId <= 0)
        {
            return CampusErrors.MissingId;
        }

        var campus = await campusRepository.GetByIdAsync(model.CampusId, cancellationToken);
        if (campus is null)
        {
            return CampusErrors.NotFound(model.CampusId);
        }

        var now = ParisTime.Now(time);
        var rooms = await roomRepository.GetByCampusIdAsync(campus.Id, cancellationToken);
        var statuses = await Task.WhenAll(rooms.Select(room => GetStatusAsync(room, now, cancellationToken)));

        return RoomMapper.ToGetRoomsStatusResponse(campus, now, statuses);
    }

    private async Task<RoomStatusResponse> GetStatusAsync(RoomEntity room, DateTime now, CancellationToken cancellationToken)
    {
        var timetable = await timetableService.GetAsync(room.ResourceId, options.RoomMaxAge, cancellationToken);
        return timetable.Match(
            snapshot => RoomMapper.ToStatusResponse(RoomAvailability.Compute(
                room,
                snapshot.Courses,
                DateOnly.FromDateTime(now),
                TimeOnly.FromDateTime(now))),
            _ => RoomMapper.ToUnknownStatusResponse(room));
    }
}
