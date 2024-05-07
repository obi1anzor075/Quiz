﻿using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.DataContext;
using System.Linq;

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
        {            Question nextQuestion = _dbContext.Questions.Skip(index).FirstOrDefault();

            // Проверка, существует ли сессионная переменная CurrentQuestionIndex
            if (!HttpContext.Session.TryGetValue("CurrentQuestionId", out byte[] questionIdBytes))
            {
                // Если нет, установите его в 0
                HttpContext.Session.SetInt32("CurrentQuestionId", 0);
            }

            // Проверка, если пользователь пытается вернуться на предыдущий вопрос
            if (  nextQuestion.QuestionId != HttpContext.Session.GetInt32("CurrentQuestionId"))
            {
                // Сбросить состояние, если пользователь пытается вернуться на предыдущий вопрос
                HttpContext.Session.SetInt32("CurrentQuestionId", nextQuestion.QuestionId);
            }
            // Получаем следующий вопрос из базы данных по переданному индексу

            if (nextQuestion != null)
            {
                ViewBag.QuestionId = nextQuestion.QuestionId;
                ViewBag.QuestionText = nextQuestion.QuestionText;
                ViewBag.ImageUrl = nextQuestion.ImageUrl;
                ViewBag.Answer1 = nextQuestion.Answer1;
                ViewBag.Answer2 = nextQuestion.Answer2;
                ViewBag.Answer3 = nextQuestion.Answer3;
                ViewBag.Answer4 = nextQuestion.Answer4;

                // Увеличиваем индекс для следующего запроса
                int nextIndex = index + 1;

                // Передаем индекс следующего вопроса в представление
                ViewBag.NextIndex = nextIndex;

                return View();
            }

            // Возвращаем представление с сообщением об ошибке или что-то другое, если вопросы закончились
            return View("QuestionNotFound");
        }
    }
}
