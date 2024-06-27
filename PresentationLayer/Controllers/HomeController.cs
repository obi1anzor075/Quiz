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
using System.Security.Claims;

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

            if (result?.Principal != null)
            {
                // Extract user information from the Google response
                var claims = result.Principal.Claims.ToList();
                var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(googleId) && !string.IsNullOrEmpty(email))
                {
                    // Save the user data to the database
                    var user = new User
                    {
                        GoogleId = googleId,
                        Email = email,
                        Name = name,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _userService.SaveUserAsync(user); // Assumes SaveUserAsync is a method in IUserService to save user data

                    // Save user name in cookies
                    HttpContext.Response.Cookies.Append("userName", name, new CookieOptions { HttpOnly = true, Secure = true });
                }
            }

            return RedirectToAction("SelectMode"); // Handle the case when authentication fails
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
