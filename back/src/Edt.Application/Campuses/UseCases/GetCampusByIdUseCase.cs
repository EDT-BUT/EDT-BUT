using Edt.Application.Campuses.Mappers;
using Edt.Application.Campuses.Responses;
using Edt.Domain.Campuses;
using Edt.Domain.Formations;
using Edt.Domain.Rooms;
using Edt.Domain.Shared;

namespace Edt.Application.Campuses.UseCases;

public sealed class GetCampusByIdUseCase(
    ICampusRepository campusRepository,
    IFormationRepository formationRepository,
    IRoomRepository roomRepository) : IGetCampusByIdUseCase
{
    public async Task<Result<GetCampusByIdResponse>> Execute(int id, CancellationToken cancellationToken)
    {
        var campus = await campusRepository.GetByIdAsync(id, cancellationToken);
        if (campus is null)
        {
            return CampusErrors.NotFound(id);
        }

        var formations = await formationRepository.GetByCampusIdAsync(id, cancellationToken);
        var rooms = await roomRepository.GetByCampusIdAsync(id, cancellationToken);
        return CampusMapper.ToGetCampusByIdResponse(campus, formations, rooms);
    }
}
