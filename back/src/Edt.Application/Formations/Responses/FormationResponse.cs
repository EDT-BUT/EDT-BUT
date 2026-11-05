namespace Edt.Application.Formations.Responses;

public sealed record FormationResponse(string Id, string Name, int CampusId, IReadOnlyList<FormationYearResponse> Years);
