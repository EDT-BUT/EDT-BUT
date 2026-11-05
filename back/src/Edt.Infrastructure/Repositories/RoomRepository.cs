using Edt.Domain.Rooms;
using Edt.Infrastructure.DataBase;

namespace Edt.Infrastructure.Repositories;

public sealed class RoomRepository(CatalogContext context) : IRoomRepository
{
    private readonly Dictionary<int, RoomEntity> _roomsById = context.Rooms
        .DistinctBy(r => r.ResourceId)
        .ToDictionary(r => r.ResourceId);

    private readonly ILookup<int, RoomEntity> _roomsByCampus = context.Rooms.ToLookup(r => r.CampusId);

    public Task<RoomEntity?> GetByIdAsync(int resourceId, CancellationToken cancellationToken) =>
        Task.FromResult(_roomsById.GetValueOrDefault(resourceId));

    public Task<IReadOnlyList<RoomEntity>> GetByCampusIdAsync(int campusId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RoomEntity>>(_roomsByCampus[campusId].ToList());
}
