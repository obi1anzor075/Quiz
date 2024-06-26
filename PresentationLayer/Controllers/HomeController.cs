using Microsoft.AspNetCore.Mvc;
using PresentationLayer.Models;
using System.Diagnostics;
using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQuestionsService _questionsService;
        private readonly IUserService _userService;

        public HomeController(IQuestionsService questionsService, IUserService userService)
        {
            _questionsService = questionsService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SelectMode()
        {
            return View();
        }

        public async Task Login()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result.Principal != null)
            {
                var claims = result.Principal.Identities.FirstOrDefault().Claims;
                var name = claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var googleId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                var user = new User
                {
                    GoogleId = googleId,
                    Name = name,
                    Email = "" // Assuming email is not available during normal login
                };

                try
                {
                    await _userService.SaveUserAsync(user);
                }
                catch (Exception ex)
                {
                    // Handle exception (e.g., log or show error message)
                    return RedirectToAction("Error", "Home");
                }

                return RedirectToAction("SelectMode");
            }

            return RedirectToAction("Index"); // Handle the case when authentication fails
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
