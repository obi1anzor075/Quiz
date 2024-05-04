using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;




namespace PresentationLayer.Controllers
{
    public class SelectModeController : Controller
    {
        public IActionResult Easy()
        {
            return View();
        }
    }
}
