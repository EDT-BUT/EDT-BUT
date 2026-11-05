using Edt.Domain.Formations;
using Edt.Infrastructure.DataBase;

namespace Edt.Infrastructure.Repositories;

public sealed class FormationRepository(CatalogContext context) : IFormationRepository
{
    private readonly ILookup<int, FormationEntity> _formationsByCampus = context.Formations.ToLookup(f => f.CampusId);

    public Task<IReadOnlyList<FormationEntity>> GetByCampusIdAsync(int campusId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<FormationEntity>>(_formationsByCampus[campusId].ToList());
}
