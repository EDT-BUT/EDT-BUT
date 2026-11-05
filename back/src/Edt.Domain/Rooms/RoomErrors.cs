using Edt.Domain.Shared;

namespace Edt.Domain.Rooms;

public static class RoomErrors
{
    public static Error NotFound(int resourceId) =>
        new("Room.NotFound", $"La salle {resourceId} n'existe pas.", ErrorType.NotFound);
}
