namespace Edt.Domain.Formations;

public sealed record FormationEntity(string Id, string Name, int CampusId, IReadOnlyList<FormationYearEntity> Years);
