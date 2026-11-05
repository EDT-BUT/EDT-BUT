using Edt.Application.Timetables.Responses;

namespace Edt.Application.Rooms.Responses;

public sealed record RoomStatusResponse(
    RoomResponse Room,
    RoomState State,
    string? FreeUntil,
    string? BusyUntil,
    CourseResponse? Current,
    CourseResponse? Next);
