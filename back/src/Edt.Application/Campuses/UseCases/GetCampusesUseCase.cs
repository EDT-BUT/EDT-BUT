using Edt.Application.Campuses.Mappers;
using Edt.Application.Campuses.Responses;
using Edt.Domain.Campuses;
using Edt.Domain.Formations;
using Edt.Domain.Rooms;
using Edt.Domain.Shared;

namespace Edt.Application.Campuses.UseCases;

public sealed class GetCampusesUseCase(
    ICampusRepository campusRepository,
    IFormationRepository formationRepository,
    IRoomRepository roomRepository) : IGetCampusesUseCase
{
    public async Task<Result<IReadOnlyList<GetCampusesResponse>>> Execute(CancellationToken cancellationToken)
    {
        var responses = new List<GetCampusesResponse>();
        foreach (var campus in await campusRepository.GetAllAsync(cancellationToken))
        {
            var formations = await formationRepository.GetByCampusIdAsync(campus.Id, cancellationToken);
            var rooms = await roomRepository.GetByCampusIdAsync(campus.Id, cancellationToken);
            responses.Add(CampusMapper.ToGetCampusesResponse(campus, formations.Count, rooms.Count));
        }

        return Result.Success<IReadOnlyList<GetCampusesResponse>>(responses);
    }
}
