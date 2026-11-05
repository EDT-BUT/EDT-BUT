using Edt.Domain.Shared;

namespace Edt.Domain.Groups;

public static class GroupErrors
{
    public static Error NotFound(int resourceId) =>
        new("Group.NotFound", $"Le groupe {resourceId} n'existe pas.", ErrorType.NotFound);
}
