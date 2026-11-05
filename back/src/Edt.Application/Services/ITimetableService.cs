using Edt.Domain.Shared;

namespace Edt.Application.Services;

public interface ITimetableService
{
    Task<Result<TimetableSnapshot>> GetAsync(int resourceId, TimeSpan maxAge, CancellationToken cancellationToken);
}
