using Edt.Application.Providers.Dtos;
using Edt.Domain.Shared;

namespace Edt.Application.Providers;

public interface ICourseProvider
{
    Task<Result<IReadOnlyList<CourseDto>>> GetByResourceIdAsync(int resourceId, CancellationToken cancellationToken);
}
