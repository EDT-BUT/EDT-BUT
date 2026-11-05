using Asp.Versioning;
using Edt.Api.Extensions;
using Edt.Application.Groups.Responses;
using Edt.Application.Groups.UseCases;
using Edt.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Edt.Api.Controllers.V1;

[ApiVersion("1.0")]
public sealed class GroupController : CustomControllerBase
{
    [HttpGet("{id}/timetable")]
    [ProducesResponseType(typeof(GetGroupTimetableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetGroupTimetableResponse>> GetTimetable(
        [FromRoute] int id,
        [FromServices] IGetGroupTimetableUseCase getGroupTimetableUseCase,
        CancellationToken cancellationToken)
    {
        Result<GetGroupTimetableResponse> result = await getGroupTimetableUseCase.Execute(id, cancellationToken);
        return result.GetActionResult();
    }

    [HttpGet("{id}/calendar.ics")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/calendar")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetCalendar(
        [FromRoute] int id,
        [FromServices] IExportGroupCalendarUseCase exportGroupCalendarUseCase,
        CancellationToken cancellationToken)
    {
        Result<string> result = await exportGroupCalendarUseCase.Execute(id, cancellationToken);
        return result.GetActionResult(ics => Content(ics, "text/calendar; charset=utf-8"));
    }
}
