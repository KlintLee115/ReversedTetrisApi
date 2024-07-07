using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

public class MessageHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> GroupCounts = new();
    private static readonly ConcurrentDictionary<string, List<int>> GroupStatus = new();
    private static readonly ConcurrentDictionary<string, string> UserGroups = new();

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

        if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
        {
            // Broadcast movement data to all clients in the same group (room)
            await Clients.OthersInGroup(roomId).SendAsync("ReceiveMovement", movementData);
        }
    }

    public async Task ClearRows(int[] rows)
    {
        if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
        {
            await Clients.OthersInGroup(roomId).SendAsync("ClearRows", rows);
        }
    }

    public async Task JoinRoom(string roomId)
    {
        UserGroups[Context.ConnectionId] = roomId;
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
        if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
        {
            GroupStatus.AddOrUpdate(roomId,
                key => [], // If the group doesn't exist, create a new empty list
                (key, existingList) =>
                {
                    existingList.Remove(1);
                    return existingList;
                }
            );

            if (GroupStatus[roomId].Count == 0) await Clients.Group("c1f7d0").SendAsync("Continue");
        }
    }

    public async Task GameOver()
    {
        if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
        {
            await Clients.OthersInGroup(roomId).SendAsync("You Won");
        }
    }

    public async Task Pause()
    {
        if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
        {

            GroupStatus.AddOrUpdate(roomId,
               key => [1, 1],  // If the group doesn't exist, create a new list with "1"
               (key, existingList) =>
               {
                   existingList.Add(1);
                   existingList.Add(1);
                   return existingList;
               });

            await Clients.OthersInGroup(roomId).SendAsync("Pause");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (UserGroups.TryRemove(Context.ConnectionId, out var roomId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            GroupCounts.AddOrUpdate(roomId, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);

            await Clients.Group(roomId).SendAsync("UpdateGroupCount", GroupCounts[roomId]);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
