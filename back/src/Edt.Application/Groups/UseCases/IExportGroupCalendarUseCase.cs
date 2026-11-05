using Edt.Domain.Shared;

namespace Edt.Application.Groups.UseCases;

public interface IExportGroupCalendarUseCase
{
    Task<Result<string>> Execute(int id, CancellationToken cancellationToken);
}
