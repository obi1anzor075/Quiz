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

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
            await Clients.Group(connection.ChatRoom).ReceiveMessage("Brand-Battle", $"Добро пожаловать, {connection.UserName}!");
        }

        public async Task JoinDuel(UserConnection connection)
        {
            if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
            {
                throw new ArgumentException("Invalid connection parameters");
            }

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
            await Clients.All.ReceiveMessage(userName, message);
        }

        public async Task GetNextQuestion(string chatRoom, int questionIndex)
        {
            var nextQuestion = _dbContext.Questions.Skip(questionIndex).FirstOrDefault();
            if (nextQuestion != null)
            {
                var answers = new List<string> { nextQuestion.Answer1, nextQuestion.Answer2, nextQuestion.Answer3, nextQuestion.Answer4 };
                var random = new Random();
                for (int i = answers.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    var temp = answers[i];
                    answers[i] = answers[j];
                    answers[j] = temp;
                }

                await Clients.Group(chatRoom).ReceiveQuestion(nextQuestion.QuestionId, nextQuestion.QuestionText, nextQuestion.ImageUrl, answers);
            }
        }

        public async Task AnswerQuestion(string chatRoom, int questionId, string answer)
        {
      
                // Log the received parameters for debugging

                // Example: Retrieve the correct answer from the database
                var question = await _dbContext.Questions.FindAsync(questionId);
                if (question == null)
                {
                    throw new Exception($"Question with ID {questionId} not found.");
                }

                bool isCorrect = (answer == question.CorrectAnswer);

                // Log the result of the answer check

                // Additional logic for scoring, etc.

            
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
    }
}
