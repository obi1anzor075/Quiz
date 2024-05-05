using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;
using System.Collections.Generic;

using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;



namespace PresentationLayer.Controllers
{
    public class SelectModeController : Controller
    {
        private readonly IQuestionsService _questionsService;

        public SelectModeController(IQuestionsService questionsService)
        {
            _questionsService = questionsService;
        }
        public async Task<IActionResult> Easy()
        {
            List<Question> listQuestions = await _questionsService.GetQuestion();
            return View(listQuestions);
        }
    }
}
