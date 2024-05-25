using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PresentationLayer.Hubs
{
    public class GameHub : Hub
    {
        private static Dictionary<string, string> _players = new Dictionary<string, string>();

        public async Task JoinGame(string gameCode)
        {
            if (!_players.ContainsKey(Context.ConnectionId))
            {
                _players[Context.ConnectionId] = gameCode;
                await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
                await Clients.Groups(gameCode).SendAsync("Player Joined", Context.ConnectionId);
            }
        }

        public async Task StartGame(string gameCode)
        {
            await Clients.Group(gameCode).SendAsync("Game Started");
        }
    }
}
