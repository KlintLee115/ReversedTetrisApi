namespace ReversedTetrisApi;

public static class RoomManager
{
    public static readonly HashSet<string> UsedRoomIds = [];

    public static string GenerateUniqueRoomId()
    {
        string roomId;
        do
        {
            roomId = Guid.NewGuid().ToString("N")[..6]; // Generate a 6-character unique ID
        } while (!UsedRoomIds.Add(roomId)); // Add returns false if the ID already exists

        return roomId;
    }
}