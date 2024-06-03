using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using PresentationLayer.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PresentationLayer.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(string userName, string message);
        Task ReceiveInvitation(string fromUser);
    }

    public class GameHub : Hub<IChatClient>
    {
        private readonly IDistributedCache _cache;

        public GameHub(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task JoinChat(UserConnection connection)
        {
            if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
            {
                throw new ArgumentException("Invalid connection parameters");
            }

            Console.WriteLine($"User {connection.UserName} is joining the chat room {connection.ChatRoom}");

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
            await Clients.Group(connection.ChatRoom).ReceiveMessage("Brand-Battle", $"Добро пожаловать {connection.UserName}");

            // Save the user's connection ID to Redis
            await _cache.SetStringAsync(connection.UserName, Context.ConnectionId);
        }

        public async Task SendInvitation(string toUserName)
        {
            var fromUserName = Context.User.Identity.Name;
            var connectionId = await _cache.GetStringAsync(toUserName);
            if (connectionId != null)
            {
                await Clients.Client(connectionId).ReceiveInvitation(fromUserName);
            }
        }
    }
}