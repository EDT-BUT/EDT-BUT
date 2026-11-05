namespace Edt.Application.Campuses.Responses;

public sealed record GetCampusesResponse(int Id, string Name, int FormationCount, int RoomCount);
