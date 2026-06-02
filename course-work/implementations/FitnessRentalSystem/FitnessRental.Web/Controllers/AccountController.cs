using FitnessRental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FitnessRental.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("FitnessApi");

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "api/Auth/login",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            var responseJson =
                await response.Content.ReadAsStringAsync();

            var loginResponse =
                JsonSerializer.Deserialize<LoginResponseViewModel>(
                    responseJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (loginResponse == null ||
                string.IsNullOrEmpty(loginResponse.Token) ||
                loginResponse.User == null)
            {
                ModelState.AddModelError(
                    "",
                    "Login failed.");

                return View(model);
            }

            HttpContext.Session.SetString(
                "JWToken",
                loginResponse.Token);

            HttpContext.Session.SetString(
                "UserEmail",
                loginResponse.User.Email);

            HttpContext.Session.SetString(
                "UserRole",
                loginResponse.User.Role);

            HttpContext.Session.SetInt32(
                "UserId",
                loginResponse.User.Id);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("FitnessApi");

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "api/Auth/register",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Registration failed. Email may already be used.");

                return View(model);
            }

            TempData["Success"] =
                "Registration successful. Please login.";

            return RedirectToAction("Login", "Account");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}