using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;

using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;

namespace PresentationLayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQuestionsService _questionsService;

        public HomeController(IQuestionsService questionsService)
        {
            _questionsService = questionsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SelectMode()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
