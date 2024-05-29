﻿using Microsoft.AspNetCore.SignalR;
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

            // Вывод в консоль для отладки
            Console.WriteLine($"User {connection.UserName} is joining the chat room {connection.ChatRoom}");

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
            await Clients
                .Group(connection.ChatRoom)
                .ReceiveMessage("Brand-Battle", $"Добро пожаловать {connection.UserName}");
        }
    }
}