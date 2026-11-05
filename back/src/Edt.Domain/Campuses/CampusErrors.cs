using Edt.Domain.Shared;

namespace Edt.Domain.Campuses;

public static class CampusErrors
{
    public static Error NotFound(int id) =>
        new("Campus.NotFound", $"Le campus {id} n'existe pas.", ErrorType.NotFound);

    public static readonly Error MissingId =
        new("Campus.MissingId", "Le numéro du campus est obligatoire.", ErrorType.Validation);
}
