using Edt.Application.Formations.Responses;
using Edt.Application.Rooms.Responses;

namespace Edt.Application.Campuses.Responses;

public sealed record GetCampusByIdResponse(
    CampusResponse Campus,
    IReadOnlyList<FormationResponse> Formations,
    IReadOnlyList<RoomResponse> Rooms);
