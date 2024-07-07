using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

public class MessageHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> GroupCounts = new();
    private static readonly ConcurrentDictionary<string, List<int>> GroupStatus = new();

    public class MovementData
    {
        public required List<List<int>> PrevCoor { get; set; }
        public required List<List<int>> NewCoor { get; set; }
        public required string Color { get; set; }
    }
    public async Task SendMovement(string data)
    {
        // Deserialize JSON data received from client
        var movementData = JsonSerializer.Deserialize<MovementData>(data);

        // string roomId = Context.ConnectionId;

        // Console.WriteLine(roomId);

        // Broadcast movement data to all clients in the same group (room)
        await Clients.OthersInGroup("c1f7d0").SendAsync("ReceiveMovement", movementData);
    }

    public async Task ClearRows(int[] rows)
    {
        await Clients.OthersInGroup("c1f7d0").SendAsync("ClearRows", rows);
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        int newCount = GroupCounts.AddOrUpdate(roomId, 1, (key, oldValue) => oldValue + 1);

        await Clients.Group(roomId).SendAsync("UpdateGroupCount", newCount);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        GroupCounts.AddOrUpdate(roomId, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);

        // Notify clients about the updated group count
        await Clients.Group(roomId).SendAsync("UpdateGroupCount", GroupCounts[roomId]);
    }

    public async Task Continue()
    {
        GroupStatus.AddOrUpdate("c1f7d0",
            key => [], // If the group doesn't exist, create a new empty list
            (key, existingList) =>
            {
                existingList.Remove(1);
                return existingList;
            }
        );

        if (GroupStatus["c1f7d0"].Count == 0) await Clients.Group("c1f7d0").SendAsync("Continue");
    }

    public async Task GameOver()
    {
        await Clients.OthersInGroup("c1f7d0").SendAsync("You Won");
    }

    public async Task Pause()
    {
        GroupStatus.AddOrUpdate("c1f7d0",
           key => [1, 1],  // If the group doesn't exist, create a new list with "1"
           (key, existingList) =>
           {
               existingList.Add(1);
               existingList.Add(1);
               return existingList;
           }
       );

        Console.WriteLine(GroupStatus["c1f7d0"].Count);

        await Clients.OthersInGroup("c1f7d0").SendAsync("Pause");
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var roomId in GroupCounts.Keys)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

                GroupCounts.AddOrUpdate(roomId, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);

                Console.WriteLine(GroupCounts[roomId]);

            }
            catch (Exception ex)
            {
                // Handle any exceptions if needed, depending on your application logic
                Console.WriteLine($"Error removing connection {Context.ConnectionId} from group {roomId}: {ex.Message}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
