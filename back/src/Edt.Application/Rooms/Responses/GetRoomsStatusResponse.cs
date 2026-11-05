using Edt.Application.Campuses.Responses;

namespace Edt.Application.Rooms.Responses;

public sealed record GetRoomsStatusResponse(
    CampusResponse Campus,
    string Date,
    string Time,
    IReadOnlyList<RoomStatusResponse> Rooms);
