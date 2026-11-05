using Edt.Domain.Campuses;
using Edt.Infrastructure.DataBase;

namespace Edt.Infrastructure.Repositories;

public sealed class CampusRepository(CatalogContext context) : ICampusRepository
{
    private readonly IReadOnlyList<CampusEntity> _campuses = context.Campuses;
    private readonly Dictionary<int, CampusEntity> _campusesById = context.Campuses.ToDictionary(c => c.Id);

    public Task<IReadOnlyList<CampusEntity>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_campuses);

    public Task<CampusEntity?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_campusesById.GetValueOrDefault(id));
}
