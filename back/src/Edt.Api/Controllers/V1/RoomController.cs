using Asp.Versioning;
using Edt.Api.Extensions;
using Edt.Application.Rooms.Models;
using Edt.Application.Rooms.Responses;
using Edt.Application.Rooms.UseCases;
using Edt.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Edt.Api.Controllers.V1;

[ApiVersion("1.0")]
public sealed class RoomController : CustomControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType(typeof(GetRoomsStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetRoomsStatusResponse>> GetStatus(
        [FromQuery] GetRoomsStatusModel parameters,
        [FromServices] IGetRoomsStatusUseCase getRoomsStatusUseCase,
        CancellationToken cancellationToken)
    {
        Result<GetRoomsStatusResponse> result = await getRoomsStatusUseCase.Execute(parameters, cancellationToken);
        return result.GetActionResult();
    }

    [HttpGet("{id}/timetable")]
    [ProducesResponseType(typeof(GetRoomTimetableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetRoomTimetableResponse>> GetTimetable(
        [FromRoute] int id,
        [FromServices] IGetRoomTimetableUseCase getRoomTimetableUseCase,
        CancellationToken cancellationToken)
    {
        Result<GetRoomTimetableResponse> result = await getRoomTimetableUseCase.Execute(id, cancellationToken);
        return result.GetActionResult();
    }
}
