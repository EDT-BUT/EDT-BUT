using Edt.Domain.Formations;

namespace Edt.Domain.Groups;

public sealed record GroupPlacement(FormationEntity Formation, FormationYearEntity Year, GroupEntity Group);
