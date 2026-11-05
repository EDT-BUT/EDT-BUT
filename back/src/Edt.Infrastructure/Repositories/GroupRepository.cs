using Edt.Domain.Groups;
using Edt.Infrastructure.DataBase;

namespace Edt.Infrastructure.Repositories;

public sealed class GroupRepository(CatalogContext context) : IGroupRepository
{
    private readonly Dictionary<int, GroupPlacement> _groupsById = (
        from formation in context.Formations
        from year in formation.Years
        from @group in year.Groups
        select new GroupPlacement(formation, year, @group)
    ).ToDictionary(p => p.Group.ResourceId);

    public Task<GroupPlacement?> GetByIdAsync(int resourceId, CancellationToken cancellationToken) =>
        Task.FromResult(_groupsById.GetValueOrDefault(resourceId));
}
