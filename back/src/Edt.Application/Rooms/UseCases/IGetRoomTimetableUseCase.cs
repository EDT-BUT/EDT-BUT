using Edt.Application.Rooms.Responses;
using Edt.Domain.Shared;

namespace Edt.Application.Rooms.UseCases;

public interface IGetRoomTimetableUseCase
{
    Task<Result<GetRoomTimetableResponse>> Execute(int id, CancellationToken cancellationToken);
}
