using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ReversedTetrisApi
{
    public class MessageHub : Hub
    {

        public enum PlayerStatus
        {
            InGame,
            Paused
        }
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, PlayerStatus>> GroupStatus = new();
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

            var roomStatus = GroupStatus.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, PlayerStatus>());

            // Add or update the status of the current connection
            roomStatus[Context.ConnectionId] = PlayerStatus.InGame;

            await Clients.Group(roomId).SendAsync("UpdateGroupCount", roomStatus.Count);
        }

        public async Task Continue()
        {
            if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
            {
                if (GroupStatus.TryGetValue(roomId, out var roomStatus))
                {
                    roomStatus[Context.ConnectionId] = PlayerStatus.InGame;

                    bool allPlayersInGame = roomStatus.Values.All(status => status == PlayerStatus.InGame);

                    if (allPlayersInGame)
                    {
                        await Clients.Group(roomId).SendAsync("Continue");
                    }
                }
            }
        }

        public async Task GameOver()
        {
            if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
            {
                await Clients.OthersInGroup(roomId).SendAsync("You Won");
            }
        }

        public async Task NotifyPause()
        {
            if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
            {
                if (GroupStatus.TryGetValue(roomId, out var roomStatus))
                {
                    foreach (var playerId in roomStatus.Keys)
                    {
                        roomStatus[playerId] = PlayerStatus.Paused;
                    }
                    await Clients.OthersInGroup(roomId).SendAsync("Pause");
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (UserGroups.TryRemove(Context.ConnectionId, out string? roomId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

                if (GroupStatus.TryGetValue(roomId, out var roomStatus))
                {
                    roomStatus.TryRemove(Context.ConnectionId, out _);
                    await Clients.Group(roomId).SendAsync("LeaveGame");
                }

                await base.OnDisconnectedAsync(exception);
            }
        }
    }
}