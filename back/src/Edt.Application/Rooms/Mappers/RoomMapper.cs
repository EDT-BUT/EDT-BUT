using System.Globalization;
using Edt.Application.Campuses.Mappers;
using Edt.Application.Rooms.Responses;
using Edt.Application.Services;
using Edt.Application.Timetables.Mappers;
using Edt.Domain.Campuses;
using Edt.Domain.Rooms;

namespace Edt.Application.Rooms.Mappers;

public static class RoomMapper
{
    public static RoomResponse ToResponse(RoomEntity room) => new(room.ResourceId, room.Label, room.CampusId);

    public static GetRoomTimetableResponse ToGetRoomTimetableResponse(
        RoomEntity room,
        string campusName,
        TimetableSnapshot timetable) => new(
        ToResponse(room),
        campusName,
        CourseMapper.ToResponses(timetable.Courses),
        timetable.FetchedAt,
        timetable.IsStale);

    public static RoomStatusResponse ToStatusResponse(RoomAvailability availability) => new(
        ToResponse(availability.Room),
        availability.IsFree ? RoomState.Free : RoomState.Busy,
        availability.FreeUntil?.ToString("HH:mm", CultureInfo.InvariantCulture),
        availability.BusyUntil?.ToString("HH:mm", CultureInfo.InvariantCulture),
        availability.Current is { } current ? CourseMapper.ToResponse(current) : null,
        availability.Next is { } next ? CourseMapper.ToResponse(next) : null);

    public static RoomStatusResponse ToUnknownStatusResponse(RoomEntity room) =>
        new(ToResponse(room), RoomState.Unknown, null, null, null, null);

    public static GetRoomsStatusResponse ToGetRoomsStatusResponse(
        CampusEntity campus,
        DateTime now,
        IReadOnlyList<RoomStatusResponse> rooms) => new(
        CampusMapper.ToResponse(campus),
        now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        now.ToString("HH:mm", CultureInfo.InvariantCulture),
        rooms);
}
