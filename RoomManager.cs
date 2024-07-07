public class RoomManager
{
    public static readonly HashSet<string> usedRoomIds = [];
    public static string GenerateRoomId()
    {
        // Generate a unique room ID based on your requirements
        // Example: You can use GUIDs or generate based on timestamp or random numbers
        return Guid.NewGuid().ToString()[..6]; // Example using GUIDs
    }
}
