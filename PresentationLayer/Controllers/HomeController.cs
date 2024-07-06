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
using Microsoft.AspNetCore.Authorization;
using PresentationLayer.ViewModels;
using Microsoft.AspNetCore.Identity;
using PresentationLayer.ErrorDescriber;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PresentationLayer.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly LocalizedIdentityErrorDescriber _localizedIdentityErrorDescriber;


        public HomeController(SignInManager<User> signInManager, UserManager<User> userManager, LocalizedIdentityErrorDescriber localizedIdentityErrorDescriber)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _localizedIdentityErrorDescriber = localizedIdentityErrorDescriber;

        }

        public IActionResult Login()
        {
            return View();
        }





        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName!, model.Password!, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName!);
                    if (user != null)
                    {
                        HttpContext.Response.Cookies.Append("userName", user.UserName, new CookieOptions { HttpOnly = true, Secure = true });
                    }
                    return RedirectToAction("SelectMode", "Home");
                }
                ModelState.AddModelError(string.Empty, _localizedIdentityErrorDescriber.InvalidLogin().Description);
                return View(model);
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    UserName = model.Email,
                    Name = model.UserName,
                    Email = model.Email,
                    CreatedAt = DateTime.Now,
                };
                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    HttpContext.Response.Cookies.Append("userName", user.Name!, new CookieOptions { HttpOnly = true, Secure = true });
                    return RedirectToAction("SelectMode", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Home");
        }

        [Authorize]
        public IActionResult SelectMode()
        {
            return View();
        }

        // Delimetrer string
        static string GetFirstWord(string input)
        {
            // Разделяем строку по пробелам и берем первое слово
            string[] words = input.Split(' ');
            if (words.Length > 0)
            {
                return words[0];
            }
            return string.Empty;
        }

        [AllowAnonymous]
        public IActionResult LoginGoogle()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Home");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            string userName = info.Principal.FindFirst(ClaimTypes.Name)?.Value.Split(' ')[0];
            string[] userInfo = { userName, info.Principal.FindFirst(ClaimTypes.Email).Value };

            if (result.Succeeded)
            {
                SaveUserNameInCookie(userName);
                return RedirectToAction("SelectMode");
            }

            // Создаем нового пользователя, если он не найден
            var user = new User
            {
                Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                UserName = info.Principal.FindFirst(ClaimTypes.Email).Value,
                GoogleId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value,
                CreatedAt = DateTime.UtcNow,
                Name = userName
            };

            // Генерируем фиктивный пароль и хэшируем его
            var dummyPassword = "DummyPassword123!";
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, dummyPassword);

            // Создаем пользователя
            var identResult = await _userManager.CreateAsync(user);
            if (identResult.Succeeded)
            {
                identResult = await _userManager.AddLoginAsync(user, info);
                if (identResult.Succeeded)
                {
                    // Входим пользователя и перенаправляем на SelectMode
                    await _signInManager.SignInAsync(user, false);
                    SaveUserNameInCookie(user.Name);
                    return RedirectToAction("SelectMode");
                }
            }

            // Если не удалось создать пользователя или добавить вход через внешний сервис, перенаправляем на SelectMode
            return RedirectToAction("SelectMode");
        }

        private void SaveUserNameInCookie(string userName)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(30),
                HttpOnly = true, 
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            if (HttpContext.Request.Cookies.ContainsKey("UserName"))
            {
                HttpContext.Response.Cookies.Delete("UserName");
            }

            HttpContext.Response.Cookies.Append("UserName", userName, cookieOptions);
        }

        [HttpGet]
        public IActionResult GetUserName()
        {
            if (HttpContext.Request.Cookies.TryGetValue("userName", out var userName))
            {
                return Json(new { userName });
            }
            return Json(new { userName = "" });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
