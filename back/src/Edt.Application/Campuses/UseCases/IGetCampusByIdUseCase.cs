using Edt.Application.Campuses.Responses;
using Edt.Domain.Shared;

namespace Edt.Application.Campuses.UseCases;

public interface IGetCampusByIdUseCase
{
    Task<Result<GetCampusByIdResponse>> Execute(int id, CancellationToken cancellationToken);
}
