using Edt.Domain.Groups;

namespace Edt.Domain.Formations;

public sealed record FormationYearEntity(int Year, string Label, IReadOnlyList<GroupEntity> Groups);
