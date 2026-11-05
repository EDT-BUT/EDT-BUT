using Edt.Application.Campuses.Responses;
using Edt.Application.Formations.Mappers;
using Edt.Application.Rooms.Mappers;
using Edt.Domain.Campuses;
using Edt.Domain.Formations;
using Edt.Domain.Rooms;

namespace Edt.Application.Campuses.Mappers;

public static class CampusMapper
{
    public static CampusResponse ToResponse(CampusEntity campus) => new(campus.Id, campus.Name);

    public static GetCampusesResponse ToGetCampusesResponse(CampusEntity campus, int formationCount, int roomCount) =>
        new(campus.Id, campus.Name, formationCount, roomCount);

    public static GetCampusByIdResponse ToGetCampusByIdResponse(
        CampusEntity campus,
        IEnumerable<FormationEntity> formations,
        IEnumerable<RoomEntity> rooms) => new(
        ToResponse(campus),
        formations.Select(FormationMapper.ToResponse).ToList(),
        rooms.Select(RoomMapper.ToResponse).ToList());
}
