using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using DataAccessLayer.DataContext;
using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;

namespace PresentationLayer.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(string userName, string message);
        Task StartGame();
        Task EndGame(Dictionary<string, int> results);
        Task GameReady();
        Task ReceiveQuestion(int questionId, string questionText, string imageUrl, List<string> answers);
        Task AnswerResult(bool isCorrect);
        Task RoomFull();
    }

    public class GameHub : Hub<IChatClient>
    {
        private readonly IDistributedCache _cache;
        private readonly DataStoreDbContext _dbContext;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private static readonly Dictionary<string, GameRoom> Rooms = new();
        private static readonly Dictionary<string, Timer> RoomTimers = new();

        public GameHub(IDistributedCache cache, DataStoreDbContext dbContext, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _cache = cache;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public Task<string> GetUserName()
        {
            var userName = _httpContextAccessor.HttpContext.Request.Cookies["userName"];
            if (string.IsNullOrEmpty(userName))
            {
                throw new KeyNotFoundException("User name not found");
            }

            return Task.FromResult(userName);
        }

        public async Task JoinChat(UserConnection connection)
        {
            if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
            {
                throw new ArgumentException("Invalid connection parameters");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
            await Clients.Group(connection.ChatRoom).ReceiveMessage("Quizik", $"Добро пожаловать, {connection.UserName}!");
        }

        // Save UserName and GET
        /*public async Task SaveUserName()
          {
              var userName = Context.GetHttpContext().Request.Cookies["userName"];
              if (string.IsNullOrEmpty(userName))
              {
                  throw new ArgumentException("Invalid user name");
              }

              // Create user object with the received username
              var user = new User
              {
                  Name = userName,
                  GoogleId = null, // Assuming no GoogleId for non-Google login
                  Email = "user@example.com", // Replace with actual email if available
                  CreatedAt = DateTime.UtcNow
              };

              // Save the user to the database
              await _userService.SaveUserAsync(user);
          }
        */
        public async Task JoinDuel(UserConnection connection)
        {
            try
            {
                if (connection == null || string.IsNullOrEmpty(connection.UserName) || string.IsNullOrEmpty(connection.ChatRoom))
                {
                    throw new ArgumentException("Invalid connection parameters");
                }

                var duel = await GetOrCreateDuel(connection.ChatRoom);

                if (!string.IsNullOrEmpty(duel.Player1) && !string.IsNullOrEmpty(duel.Player2))
                {
                    await Clients.Caller.RoomFull();
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);

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
                    if (!Rooms.ContainsKey(connection.ChatRoom))
                    {
                        Rooms[connection.ChatRoom] = new GameRoom(connection.ChatRoom);
                    }

                    Rooms[connection.ChatRoom].StartGame();

                    await Clients.Group(connection.ChatRoom).GameReady();
                    await Clients.Group(connection.ChatRoom).StartGame();
                }

                var playerState = await GetPlayerState(connection.UserName);
                if (playerState == null)
                {
                    await SavePlayerState(connection.UserName, new PlayerState());
                    Console.WriteLine($"User {connection.UserName} added to player states.");
                }
                else
                {
                    Console.WriteLine($"User {connection.UserName} already exists in player states.");
                }
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                Console.WriteLine($"Redis connection error in JoinDuel: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                // Add any specific handling or retry logic here if necessary
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinDuel: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }



        private void StartRoomTimer(string chatRoom)
        {
            if (RoomTimers.ContainsKey(chatRoom))
            {
                RoomTimers[chatRoom].Dispose();
            }

            var timer = new Timer(async _ => await ClearRoom(chatRoom), null, TimeSpan.FromMinutes(10), Timeout.InfiniteTimeSpan);
            RoomTimers[chatRoom] = timer;
        }

        private async Task ClearRoom(string chatRoom)
        {
            await ClearDuel(chatRoom);
            RoomTimers.Remove(chatRoom);
        }

        public async Task SendMessage(string userName, string message)
        {
            await Clients.All.ReceiveMessage(userName, message);
        }

        public async Task AnswerQuestion(string userName, string chatRoom, int questionId, string answer)
        {
            try
            {
                Console.WriteLine($"AnswerQuestion called with UserName={userName}, ChatRoom={chatRoom}, QuestionId={questionId}, Answer={answer}");

                var playerState = await GetPlayerState(userName);
                if (playerState == null)
                {
                    throw new KeyNotFoundException($"User '{userName}' not found in player states.");
                }

                var question = _dbContext.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                if (question == null)
                {
                    throw new Exception($"Question with ID {questionId} not found.");
                }

                var isCorrect = string.Equals(answer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    playerState.Score++;
                }

                await SavePlayerState(userName, playerState);

                await Clients.Caller.AnswerResult(isCorrect);

                // Проверяем, остались ли еще вопросы
                var questionsCount = _dbContext.Questions.Count();
                if (playerState.CurrentQuestionIndex >= questionsCount)
                {
                    await EndGame(chatRoom);
                }
                else
                {
                    await GetNextQuestion(userName, chatRoom, playerState.CurrentQuestionIndex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AnswerQuestion: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public async Task GetNextQuestion(string userName, string chatRoom, int questionIndex)
        {
            try
            {
                Console.WriteLine($"GetNextQuestion called with UserName={userName}, ChatRoom={chatRoom}, QuestionIndex={questionIndex}");

                var playerState = await GetPlayerState(userName) ?? new PlayerState();
                await SavePlayerState(userName, playerState);

                var questionsCount = _dbContext.Questions.Count();
                if (questionIndex >= questionsCount)
                {
                    await EndGame(chatRoom);
                    return;
                }

                var nextQuestion = _dbContext.Questions
                    .OrderBy(q => q.QuestionId)
                    .Skip(questionIndex)
                    .FirstOrDefault();

                if (nextQuestion != null)
                {
                    var answers = new List<string> { nextQuestion.Answer1, nextQuestion.Answer2, nextQuestion.Answer3, nextQuestion.Answer4 };

                    playerState.CurrentQuestionIndex = questionIndex + 1; // Обновляем индекс следующего вопроса
                    await SavePlayerState(userName, playerState);

                    await Clients.Client(Context.ConnectionId).ReceiveQuestion(nextQuestion.QuestionId, nextQuestion.QuestionText, nextQuestion.ImageUrl, answers);
                    Console.WriteLine($"Question sent: {nextQuestion.QuestionId} - {nextQuestion.QuestionText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextQuestion: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

public async Task EndGame(string chatRoom)
{
    try
    {
        // Проверяем, активна ли игра для данной комнаты
        if (!Rooms.ContainsKey(chatRoom) || !Rooms[chatRoom].IsGameActive)
        {
            Console.WriteLine($"EndGame already processed or game not active for chatRoom {chatRoom}");
            return;
        }

        var duel = await GetOrCreateDuel(chatRoom);
        var results = new Dictionary<string, int>();

        if (duel != null)
        {
            if (!string.IsNullOrEmpty(duel.Player1))
            {
                var player1State = await GetPlayerState(duel.Player1);
                if (player1State != null)
                {
                    results[duel.Player1] = player1State.Score;
                }
            }

            if (!string.IsNullOrEmpty(duel.Player2))
            {
                var player2State = await GetPlayerState(duel.Player2);
                if (player2State != null)
                {
                    results[duel.Player2] = player2State.Score;
                }
            }

            // Log the results before sending
            Console.WriteLine("Results before sending to client:");
            foreach (var result in results)
            {
                Console.WriteLine($"Player: {result.Key}, Score: {result.Value}");
            }

            await Clients.Group(chatRoom).EndGame(results);

            // Set IsGameActive to false so EndGame cannot be called again
            Rooms[chatRoom].IsGameActive = false;

            // Clear the duel state
            await ClearDuel(chatRoom);

            // Clear the players' states
            if (!string.IsNullOrEmpty(duel.Player1))
            {
                await ClearPlayerState(duel.Player1);
            }
            if (!string.IsNullOrEmpty(duel.Player2))
            {
                await ClearPlayerState(duel.Player2);
            }
        }
        else
        {
            Console.WriteLine($"Error: Duel not found for chatRoom {chatRoom}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in EndGame: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw;
    }
}



        private async Task ClearDuel(string chatRoom)
        {
            try
            {
                var cacheKey = $"{chatRoom}:duel";
                await _cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ClearDuel: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private async Task ClearPlayerState(string userName)
        {
            try
            {
                var cacheKey = $"playerState:{userName}";
                await _cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ClearPlayerState: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private async Task<PlayerState> GetPlayerState(string userName)
        {
            try
            {
                var cacheKey = $"playerState:{userName}";
                var playerStateJson = await _cache.GetStringAsync(cacheKey);
                if (playerStateJson != null)
                {
                    return JsonConvert.DeserializeObject<PlayerState>(playerStateJson);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPlayerState: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private async Task SavePlayerState(string userName, PlayerState playerState)
        {
            try
            {
                var cacheKey = $"playerState:{userName}";
                var playerStateJson = JsonConvert.SerializeObject(playerState);
                await _cache.SetStringAsync(cacheKey, playerStateJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SavePlayerState: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private async Task<Duel> GetOrCreateDuel(string chatRoom)
        {
            try
            {
                var cacheKey = $"{chatRoom}:duel";
                var duelJson = await _cache.GetStringAsync(cacheKey);
                if (duelJson != null)
                {
                    return JsonConvert.DeserializeObject<Duel>(duelJson);
                }

                return new Duel(string.Empty, string.Empty, chatRoom, new Dictionary<string, int>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOrCreateDuel: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private async Task SaveDuel(string chatRoom, Duel duel)
        {
            try
            {
                var cacheKey = $"{chatRoom}:duel";
                var duelJson = JsonConvert.SerializeObject(duel);
                await _cache.SetStringAsync(cacheKey, duelJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveDuel: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private class PlayerState
        {
            public int CurrentQuestionIndex { get; set; } = 0;
            public int Score { get; set; } = 0;
        }

        public class GameRoom
        {
            public string RoomName { get; set; }
            public List<string> Players { get; set; }
            public bool IsGameActive { get; set; }

            public GameRoom(string roomName)
            {
                RoomName = roomName;
                Players = new List<string>();
                IsGameActive = false; // Изначально игра не активна
            }

            public void StartGame()
            {
                IsGameActive = true;
            }
        }


        public record Duel(string Player1, string Player2, string ChatRoom, Dictionary<string, int> Scores);
    }
}
