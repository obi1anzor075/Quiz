using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;

using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;

namespace PresentationLayer.Controllers
{
    public class GameController : Controller
    {
        [HttpPost]
        public IActionResult CheckAnswer(string selectedAnswer)
        {
            // Проведите проверку правильности ответа
            bool isCorrect = true; // Здесь PerformAnswerCheck - ваша логика проверки ответа

            // Верните результат проверки обратно на клиент
            return Json(new { isCorrect = isCorrect });
        }
    }
}
