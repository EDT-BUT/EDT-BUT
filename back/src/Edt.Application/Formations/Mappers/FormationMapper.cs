using Edt.Application.Formations.Responses;
using Edt.Application.Groups.Mappers;
using Edt.Domain.Formations;

namespace Edt.Application.Formations.Mappers;

public static class FormationMapper
{
    public static FormationResponse ToResponse(FormationEntity formation) => new(
        formation.Id,
        formation.Name,
        formation.CampusId,
        formation.Years.Select(ToYearResponse).ToList());

    public static FormationYearResponse ToYearResponse(FormationYearEntity year) => new(
        year.Year,
        year.Label,
        year.Groups.Select(GroupMapper.ToResponse).ToList());
}
