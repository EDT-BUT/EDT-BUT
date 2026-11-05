using Edt.Application.Campuses.Responses;
using Edt.Domain.Shared;

namespace Edt.Application.Campuses.UseCases;

public interface IGetCampusesUseCase
{
    Task<Result<IReadOnlyList<GetCampusesResponse>>> Execute(CancellationToken cancellationToken);
}
