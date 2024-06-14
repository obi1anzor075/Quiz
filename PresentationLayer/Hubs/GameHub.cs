using Microsoft.AspNetCore.SignalR;
using PresentationLayer.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using DataAccessLayer.DataContext;
using System.Linq;

namespace PresentationLayer.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(string userName, string message);
        Task StartGame();
        Task GameEnded(Dictionary<string, int> results);
        Task GameReady();
        Task ReceiveQuestion(int questionId, string questionText, string imageUrl, List<string> answers);
        Task AnswerResult(bool isCorrect);
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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinDuel: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
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

        bool isCorrect = (answer.ToUpper().Trim().Normalize() == question.CorrectAnswer.ToUpper().Trim().Normalize());

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

        var playerState = await GetPlayerState(userName);
        if (playerState == null)
        {
            playerState = new PlayerState();
            await SavePlayerState(userName, playerState);
        }

        var questionsCount = _dbContext.Questions.Count();
        if (questionIndex >= questionsCount)
        {
            await EndGame(chatRoom); // Завершаем игру, если вопросы закончились
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

private async Task EndGame(string chatRoom)
{
    try
    {
        var duel = await GetOrCreateDuel(chatRoom);
        var results = new Dictionary<string, int>();

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

        await Clients.Group(chatRoom).GameEnded(results);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in EndGame: {ex.Message}");
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
    }
}
