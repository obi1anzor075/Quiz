using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Controllers
{
    public class SelectModeController : Controller
    {

        private readonly ILogger<SelectModeController> _logger;
        private readonly DataStoreDbContext _dbContext;



        public SelectModeController(DataStoreDbContext dbContext, ILogger<SelectModeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Easy(int index = 0)
        {
            try
            {
                _logger.LogInformation("Easy");
                // Проверка, существует ли сессионная переменная CurrentQuestionIndex
                if (!HttpContext.Session.TryGetValue("CurrentQuestionId", out byte[] questionIdBytes))
                {
                    // Если нет, установите его в 0
                    HttpContext.Session.SetInt32("CurrentQuestionId", 0);
                }

                // Получаем следующий вопрос из базы данных по переданному индексу
                Question nextQuestion = _dbContext.Questions.Skip(index).FirstOrDefault();

                if (nextQuestion != null)
                {
                    ViewBag.QuestionId = nextQuestion.QuestionId;
                    ViewBag.QuestionText = nextQuestion.QuestionText;
                    ViewBag.ImageUrl = nextQuestion.ImageUrl;

                    // Список ответов для перемешивания
                    var answers = new List<string> { nextQuestion.Answer1, nextQuestion.Answer2, nextQuestion.Answer3, nextQuestion.Answer4 };

                    // Перемешиваем ответы
                    var random = new Random();
                    for (int i = answers.Count - 1; i > 0; i--)
                    {
                        int j = random.Next(i + 1);
                        var temp = answers[i];
                        answers[i] = answers[j];
                        answers[j] = temp;
                    }



                    // Передаем перемешанные ответы в представление
                    ViewBag.Answers = answers;

                    // Увеличиваем индекс для следующего запроса
                    int nextIndex = index + 1;

                    // Передаем индекс следующего вопроса в представление
                    ViewBag.NextIndex = nextIndex;

                    return View();
                }

                // Если вопросы закончились, перенаправляем на страницу "Finish"
                string difficultyLevel = "Легкий";
                return RedirectToAction("Finish", new { difficultyLevel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting questions from database.");
                throw;
            }
        }

        public IActionResult Hard(int index = 0)
        {
            if (!HttpContext.Session.TryGetValue("CurrentQuestionId", out byte[] questionIdBytes))
            {
                // Если нет, установите его в 0
                HttpContext.Session.SetInt32("CurrentQuestionId", 0);
            }

            HardQuestion nextQuestion = _dbContext.HardQuestions.Skip(index).FirstOrDefault();

            if (nextQuestion != null)
            {
                ViewBag.QuestionId = nextQuestion.QuestionId;
                ViewBag.QuestionText = nextQuestion.QuestionText;
                ViewBag.ImageUrl = nextQuestion.ImageUrl;

                int nextIndex = index + 1;

                ViewBag.NextIndex = nextIndex;

                return View();
            }

            string difficultyLevel = "Сложный";
            return RedirectToAction("Finish", new { difficultyLevel });
        }

        public IActionResult JoinGame()
        {
            return View();
        }

        public IActionResult Finish(string difficultyLevel)
        {
            ViewBag.DifficultyLevel = difficultyLevel;

            // Получаем количество правильных ответов из сессии
            int correctAnswersCount = HttpContext.Session.GetInt32("CorrectAnswersCount") ?? 0;
            ViewBag.CorrectAnswersCount = correctAnswersCount;

            // Получаем общее количество вопросов
            int totalQuestionsCount = _dbContext.Questions.Count();
            ViewBag.TotalQuestionsCount = totalQuestionsCount;

            return View();
        }

        public IActionResult ResetCounters()
        {
            // Сбрасываем счетчик правильных ответов в сессии
            HttpContext.Session.SetInt32("CorrectAnswersCount", 0);

            // Сбрасываем индекс текущего вопроса в сессии
            HttpContext.Session.SetInt32("CurrentQuestionId", 0);

            return Ok();
        }
    }
}
