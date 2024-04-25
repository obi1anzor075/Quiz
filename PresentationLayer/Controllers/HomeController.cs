using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;

using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;

namespace PresentationLayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatumService _datumService;

        public HomeController(IDatumService datumService)
        {
            _datumService = datumService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> ShowDatums()
        {
            List<Datum> listDatums = await _datumService.GetDatums();
            return View(listDatums);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
