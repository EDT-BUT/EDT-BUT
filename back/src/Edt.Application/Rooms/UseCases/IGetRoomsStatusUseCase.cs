using Edt.Application.Rooms.Models;
using Edt.Application.Rooms.Responses;
using Edt.Domain.Shared;

namespace Edt.Application.Rooms.UseCases;

public interface IGetRoomsStatusUseCase
{
    Task<Result<GetRoomsStatusResponse>> Execute(GetRoomsStatusModel model, CancellationToken cancellationToken);
}
