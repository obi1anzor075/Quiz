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

            if (CurrentQuestionIndex >= _dbContext.Questions.Count())
            {
                // Если ответили на все вопросы, сбрасываем счетчик
                CurrentQuestionIndex = 0;

                // Обновляем значение в сессии
                HttpContext.Session.SetInt32("CurrentQuestionIndex", CurrentQuestionIndex);
            }

            HttpContext.Session.SetInt32("CurrentQuestionId", question.QuestionId);



            // Возвращаем результат проверки и следующий вопрос обратно на клиент
            return Json(new { isCorrect = isCorrect });
        }

       
    }
}
