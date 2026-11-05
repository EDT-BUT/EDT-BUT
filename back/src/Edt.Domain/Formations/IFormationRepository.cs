namespace Edt.Domain.Formations;

public interface IFormationRepository
{
    Task<IReadOnlyList<FormationEntity>> GetByCampusIdAsync(int campusId, CancellationToken cancellationToken);
}
