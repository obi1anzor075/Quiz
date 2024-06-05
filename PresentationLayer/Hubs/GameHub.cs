using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using DataAccessLayer.DataContext;

namespace PresentationLayer.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(string userName, string message);
        Task StartGame();
        Task GameEnded(Dictionary<string, int> results);
        Task GameReady();
        Task ReceiveQuestion(int questionId, string questionText, string imageUrl, List<string> answers);
    }

    public class GameHub : Hub<IChatClient>
    {
        private readonly IDistributedCache _cache;
        private readonly DataStoreDbContext _dbContext;

        public GameHub(IDistributedCache cache, DataStoreDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task JoinChat(UserConnection connection)
        {
            if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
            {
                throw new ArgumentException("Invalid connection parameters");
            }

            Console.WriteLine($"User {connection.UserName} is joining the chat room {connection.ChatRoom}");

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
            await Clients.Group(connection.ChatRoom).ReceiveMessage("Brand-Battle", $"Добро пожаловать, {connection.UserName}!");
        }

        public async Task JoinDuel(UserConnection connection)
        {
            if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
            {
                throw new ArgumentException("Invalid connection parameters");
            }

            Console.WriteLine($"User {connection.UserName} is joining the duel room {connection.ChatRoom}");

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);

            var duel = await GetOrCreateDuel(connection.ChatRoom);
            if (string.IsNullOrEmpty(duel.Player1))
            {
                duel = duel with { Player1 = connection.UserName };
            }
            else if (string.IsNullOrEmpty(duel.Player2))
            {
                duel = duel with { Player2 = connection.UserName };
            }

            await SaveDuel(connection.ChatRoom, duel);

            if (!string.IsNullOrEmpty(duel.Player1) && !string.IsNullOrEmpty(duel.Player2))
            {
                await Clients.Group(connection.ChatRoom).GameReady();
                await Clients.Group(connection.ChatRoom).StartGame();
            }
        }
        public async Task SendMessage(string userName, string message)
        {
            // Output to console for debugging
            // Console.WriteLine($"User {userName} is sending a message: {message}");

            // Broadcast message to all clients in the default chat room (or adjust the group as needed)
            await Clients.All.ReceiveMessage(userName, message);
        }
        public async Task SendQuestion(string chatRoom, int index)
        {
            var question = _dbContext.Questions.Skip(index).FirstOrDefault();
            if (question != null)
            {
                var answers = new List<string> { question.Answer1, question.Answer2, question.Answer3, question.Answer4 };
                var random = new Random();
                for (int i = answers.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    var temp = answers[i];
                    answers[i] = answers[j];
                    answers[j] = temp;
                }
                await Clients.Group(chatRoom).ReceiveQuestion(question.QuestionId, question.QuestionText, question.ImageUrl, answers);
            }
        }

        public async Task SendAnswer(string chatRoom, string answer)
        {
            var connectionId = Context.ConnectionId;
            await RecordAnswer(chatRoom, connectionId, answer);
        }

        public async Task EndGame(string chatRoom)
        {
            var results = await GetGameResults(chatRoom);
            await Clients.Group(chatRoom).GameEnded(results);
        }

        private async Task<Duel> GetOrCreateDuel(string chatRoom)
        {
            var cacheKey = $"{chatRoom}:duel";
            var duelJson = await _cache.GetStringAsync(cacheKey);
            if (duelJson != null)
            {
                return JsonConvert.DeserializeObject<Duel>(duelJson);
            }

            return new Duel(string.Empty, string.Empty, chatRoom, new Dictionary<string, int>());
        }

        private async Task SaveDuel(string chatRoom, Duel duel)
        {
            var cacheKey = $"{chatRoom}:duel";
            var duelJson = JsonConvert.SerializeObject(duel);
            await _cache.SetStringAsync(cacheKey, duelJson);
        }

        private async Task RecordAnswer(string chatRoom, string connectionId, string answer)
        {
            var cacheKey = $"{chatRoom}:answers:{connectionId}";
            var answersJson = await _cache.GetStringAsync(cacheKey);
            var answers = answersJson != null ? JsonConvert.DeserializeObject<List<string>>(answersJson) : new List<string>();

            answers.Add(answer);
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(answers));
        }

        private async Task<Dictionary<string, int>> GetGameResults(string chatRoom)
        {
            var cacheKey = $"{chatRoom}:duel";
            var duelJson = await _cache.GetStringAsync(cacheKey);
            var duel = duelJson != null ? JsonConvert.DeserializeObject<Duel>(duelJson) : null;

            var results = new Dictionary<string, int>();

            if (duel != null)
            {
                foreach (var player in new[] { duel.Player1, duel.Player2 })
                {
                    var answersKey = $"{chatRoom}:answers:{player}";
                    var answersJson = await _cache.GetStringAsync(answersKey);
                    var answers = answersJson != null ? JsonConvert.DeserializeObject<List<string>>(answersJson) : new List<string>();

                    // Здесь должна быть ваша логика для подсчета правильных ответов
                    int correctAnswers = 0;
                    foreach (var answer in answers)
                    {
                        // Предположим, что функция CheckAnswer() проверяет правильность ответа
                        if (CheckAnswer(answer))
                        {
                            correctAnswers++;
                        }
                    }

                    results[player] = correctAnswers;
                }
            }

            return results;
        }

        private bool CheckAnswer(string answer)
        {
            //TODO
            //  логика проверки ответа
            return true; // Заменить это на фактическую логику проверки
        }
    }
}
