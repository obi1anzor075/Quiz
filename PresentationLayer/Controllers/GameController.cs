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
        private int currentQuestionIndex = 0; // индекс текущего вопроса

        public GameController(DataStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("/Game/CheckAnswer/{selectedAnswer}")]
        public IActionResult CheckAnswer(string selectedAnswer)
        {
            // Получаем следующий вопрос из базы данных
            Question question = _dbContext.Questions.Skip(currentQuestionIndex).FirstOrDefault();

            // Увеличиваем индекс для следующего запроса
            currentQuestionIndex++;

            // Проверяем, совпадает ли выбранный ответ с правильным ответом
            bool isCorrect = (selectedAnswer == question.CorrectAnswer);

            // Возвращаем результат проверки обратно на клиент
            return Json(new { isCorrect = isCorrect });
        }
    }
}
