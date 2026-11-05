using Edt.Application.Groups.Responses;
using Edt.Domain.Shared;

namespace Edt.Application.Groups.UseCases;

public interface IGetGroupTimetableUseCase
{
    Task<Result<GetGroupTimetableResponse>> Execute(int id, CancellationToken cancellationToken);
}
