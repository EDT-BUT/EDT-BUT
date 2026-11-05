using Edt.Application.Groups.Responses;

namespace Edt.Application.Formations.Responses;

public sealed record FormationYearResponse(int Year, string Label, IReadOnlyList<GroupResponse> Groups);
