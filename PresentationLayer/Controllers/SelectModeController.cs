using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;
using System.Collections.Generic;

using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.DataContext;



namespace PresentationLayer.Controllers
{
    public class SelectModeController : Controller
    {
        private readonly IQuestionsService _questionsService;

        private readonly DataStoreDbContext _dbContext;
        private int currentQuestionIndex = 0; // индекс текущего вопроса

        public SelectModeController(IQuestionsService questionsService, DataStoreDbContext dbContext)
        {
            _questionsService = questionsService;
            _dbContext = dbContext;
        }


        public async Task<IActionResult> Easy()
        {
            Question nextQuestion = _dbContext.Questions.Skip(currentQuestionIndex).FirstOrDefault();



                ViewBag.QuestionId = nextQuestion.QuestionId;
                ViewBag.QuestionText = nextQuestion.QuestionText;
                ViewBag.ImageUrl = nextQuestion.ImageUrl;
                ViewBag.Answer1 = nextQuestion.Answer1;
                ViewBag.Answer2 = nextQuestion.Answer2;
                ViewBag.Answer3 = nextQuestion.Answer3;
                ViewBag.Answer4 = nextQuestion.Answer4;




            // Возвращаем следующий вопрос на клиент
            return View();
        }
    }
}
