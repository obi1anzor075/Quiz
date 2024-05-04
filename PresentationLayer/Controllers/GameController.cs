using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class GameController : Controller
    {
        [HttpPost]
        public IActionResult CheckAnswer(string selectedAnswer)
        {
            // Проведите проверку правильности ответа
            bool isCorrect = true; // Здесь  будет логика проверки ответа

            // Верните результат проверки обратно на клиент
            return Json(new { isCorrect = isCorrect });
        }
    }
}
