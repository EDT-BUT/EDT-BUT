using Asp.Versioning;
using Edt.Api.Extensions;
using Edt.Application.Campuses.Responses;
using Edt.Application.Campuses.UseCases;
using Edt.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Edt.Api.Controllers.V1;

[ApiVersion("1.0")]
public sealed class CampusController : CustomControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GetCampusesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<GetCampusesResponse>>> GetAll(
        [FromServices] IGetCampusesUseCase getCampusesUseCase,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<GetCampusesResponse>> result = await getCampusesUseCase.Execute(cancellationToken);
        return result.GetActionResult();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetCampusByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCampusByIdResponse>> GetById(
        [FromRoute] int id,
        [FromServices] IGetCampusByIdUseCase getCampusByIdUseCase,
        CancellationToken cancellationToken)
    {
        Result<GetCampusByIdResponse> result = await getCampusByIdUseCase.Execute(id, cancellationToken);
        return result.GetActionResult();
    }
}
