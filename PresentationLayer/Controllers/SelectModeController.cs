using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.DataContext;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PresentationLayer.Controllers
{
    public class SelectModeController : Controller
    {
        private readonly DataStoreDbContext _dbContext;

        public SelectModeController(DataStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Easy(int index = 0)
        {
            // Получаем следующий вопрос из базы данных по переданному индексу
            Question nextQuestion = _dbContext.Questions.Skip(index).FirstOrDefault();

            if (nextQuestion != null)
            {
                ViewBag.QuestionId = nextQuestion.QuestionId;
                ViewBag.QuestionText = nextQuestion.QuestionText;
                ViewBag.ImageUrl = nextQuestion.ImageUrl;
                ViewBag.Answer1 = nextQuestion.Answer1;
                ViewBag.Answer2 = nextQuestion.Answer2;
                ViewBag.Answer3 = nextQuestion.Answer3;
                ViewBag.Answer4 = nextQuestion.Answer4;

                // Хеширование правильного ответа
                string correctAnswer = nextQuestion.CorrectAnswer;
                string encodedCorrectAnswer = CalculateSHA256Hash(correctAnswer);

                // Передаем закодированный ответ в представление
                ViewBag.EncodedCorrectAnswer = encodedCorrectAnswer;

                // Увеличиваем индекс для следующего запроса
                int nextIndex = index + 1;

                // Передаем индекс следующего вопроса в представление
                ViewBag.NextIndex = nextIndex;

                return View();
            }

            // Возвращаем представление с сообщением об ошибке или что-то другое, если вопросы закончились
            return View("QuestionNotFound");
        }

        // Метод для хеширования текста с использованием SHA256
        private string CalculateSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
