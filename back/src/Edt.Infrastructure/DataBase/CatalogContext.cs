using System.Text.Json;
using Edt.Domain.Campuses;
using Edt.Domain.Formations;
using Edt.Domain.Rooms;

namespace Edt.Infrastructure.DataBase;

public sealed class CatalogContext
{
    private CatalogContext(CatalogFile file)
    {
        Campuses = file.Campuses;
        Formations = file.Formations;
        Rooms = file.Rooms;
    }

    public IReadOnlyList<CampusEntity> Campuses { get; }

    public IReadOnlyList<FormationEntity> Formations { get; }

    public IReadOnlyList<RoomEntity> Rooms { get; }

    public static CatalogContext Load(string path)
    {
        using var stream = File.OpenRead(path);
        var file = JsonSerializer.Deserialize<CatalogFile>(stream, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException($"Catalogue illisible : {path}");
        return new CatalogContext(file);
    }

    private sealed record CatalogFile(
        IReadOnlyList<CampusEntity> Campuses,
        IReadOnlyList<FormationEntity> Formations,
        IReadOnlyList<RoomEntity> Rooms);
}
