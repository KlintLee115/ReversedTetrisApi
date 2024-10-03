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
            Paused,
            ReadyToBegin
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

            var roomStatus = GroupStatus.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, PlayerStatus>());

            if (roomStatus.Any(s => s.Value != PlayerStatus.ReadyToBegin))
            {
                await Clients.Caller.SendAsync("GameStarted", "The game has already started. You cannot join.");
                return;
            }

            UserGroups[Context.ConnectionId] = roomId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            if (roomStatus.IsEmpty)
            {
                // If no one is in the room, set the current player's status to "WaitingToStart"
                roomStatus[Context.ConnectionId] = PlayerStatus.ReadyToBegin;
            }
            else
            {
                // If someone is in the room, check their status
                var otherPlayerStatus = roomStatus.FirstOrDefault().Value;

                if (otherPlayerStatus == PlayerStatus.ReadyToBegin)
                {
                    roomStatus[Context.ConnectionId] = PlayerStatus.InGame;

                    var otherPlayerId = roomStatus.First().Key;
                    roomStatus[otherPlayerId] = PlayerStatus.InGame;

                    // Notify that the game should start
                    await Clients.Group(roomId).SendAsync("PreGameCountdownShouldStart");
                }
                else
                {
                    // If the other player is not "WaitingToStart", set self to "InGame" without starting the game
                    roomStatus[Context.ConnectionId] = PlayerStatus.ReadyToBegin;

                }
            }
        }

        public async Task RequestContinue()
        {
            if (UserGroups.TryGetValue(Context.ConnectionId, out string? roomId))
            {
                if (GroupStatus.TryGetValue(roomId, out var roomStatus))
                {
                    roomStatus[Context.ConnectionId] = PlayerStatus.ReadyToBegin;

                    bool allPlayersReady = roomStatus.Values.All(status => status == PlayerStatus.ReadyToBegin);

                    if (allPlayersReady)
                    {
                        foreach (var playerId in roomStatus.Keys)
                        {
                            roomStatus[playerId] = PlayerStatus.InGame;
                        }
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