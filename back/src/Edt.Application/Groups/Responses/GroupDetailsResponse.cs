namespace Edt.Application.Groups.Responses;

public sealed record GroupDetailsResponse(
    int ResourceId,
    string Label,
    string Parent,
    string FormationId,
    string FormationName,
    int Year,
    string YearLabel,
    int CampusId,
    string CampusName);
