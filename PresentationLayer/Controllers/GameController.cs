using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PresentationLayer.Controllers
{
    public class GameController : Controller
    {
        private readonly DataStoreDbContext _dbContext;

        public GameController(DataStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int CurrentQuestionIndex
        {
            get
            {
                return HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            }
            set
            {
                HttpContext.Session.SetInt32("CurrentQuestionIndex", value);
            }
        }

        [HttpGet("/Game/CheckAnswer/{selectedAnswer}")]
        public IActionResult CheckAnswer(string selectedAnswer)
        {
            // Получаем текущий вопрос из базы данных
            Question question = _dbContext.Questions.Skip(CurrentQuestionIndex).FirstOrDefault();

            // Проверяем, совпадает ли выбранный ответ с правильным ответом
            bool isCorrect = (selectedAnswer == question.CorrectAnswer);
            CurrentQuestionIndex++;

            // Проверяем правильность ответа и увеличиваем счетчик, если ответ правильный
            if (isCorrect)
            {
                // Увеличиваем счетчик правильных ответов в сессии
                int correctAnswersCount = HttpContext.Session.GetInt32("CorrectAnswersCount") ?? 0;
                correctAnswersCount++;
                HttpContext.Session.SetInt32("CorrectAnswersCount", correctAnswersCount);
            }

            // Если ответили на все вопросы, сбрасываем счетчик
            if (CurrentQuestionIndex >= _dbContext.Questions.Count())
            {
                CurrentQuestionIndex = 0;
                HttpContext.Session.SetInt32("CurrentQuestionIndex", CurrentQuestionIndex);
            }

            HttpContext.Session.SetInt32("CurrentQuestionId", question.QuestionId);

            // Возвращаем результат проверки и следующий вопрос обратно на клиент
            return Json(new { isCorrect = isCorrect });
        }


        public IActionResult ResetCounters()
        {
            // Сброс счетчика верных ответов и CurrentQuestionId
            HttpContext.Session.Remove("CorrectAnswersCount");
            HttpContext.Session.Remove("CurrentQuestionId");
            HttpContext.Session.Remove("CurrentQuestionIndex");

            return Ok();
        }


    }
}
