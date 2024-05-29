using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Models;
using System;
using System.Threading.Tasks;

namespace PresentationLayer.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(string userName, string message);
    }

    public class GameHub : Hub<IChatClient>
    {
        public async Task JoinChat(UserConnection connection)
        {
            if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
            {
                throw new ArgumentException("Invalid connection parameters");
            }

            // Output to console for debugging
            Console.WriteLine($"User {connection.UserName} is joining the chat room {connection.ChatRoom}");

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
            await Clients
                .Group(connection.ChatRoom)
                .ReceiveMessage("Brand-Battle", $"Добро пожаловать {connection.UserName}");
        }

        public async Task SendMessage(string userName, string message)
        {
            // Output to console for debugging
            Console.WriteLine($"User {userName} is sending a message: {message}");

            // Broadcast message to all clients in the default chat room (or adjust the group as needed)
            await Clients.All.ReceiveMessage(userName, message);
        }
    }
}
