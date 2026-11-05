namespace Edt.Domain.Campuses;

public interface ICampusRepository
{
    Task<IReadOnlyList<CampusEntity>> GetAllAsync(CancellationToken cancellationToken);

    Task<CampusEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
