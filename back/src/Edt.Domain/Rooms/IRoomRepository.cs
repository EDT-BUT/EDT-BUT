namespace Edt.Domain.Rooms;

public interface IRoomRepository
{
    Task<RoomEntity?> GetByIdAsync(int resourceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RoomEntity>> GetByCampusIdAsync(int campusId, CancellationToken cancellationToken);
}
