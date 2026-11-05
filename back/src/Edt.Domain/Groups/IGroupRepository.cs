namespace Edt.Domain.Groups;

public interface IGroupRepository
{
    Task<GroupPlacement?> GetByIdAsync(int resourceId, CancellationToken cancellationToken);
}
