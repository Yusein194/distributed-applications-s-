using FitnessRental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FitnessRental.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("FitnessApi");

            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("JWToken"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        public async Task<IActionResult> Index(
    string? firstName,
    string? lastName,
    string? email,
    string? role,
    int pageNumber = 1,
    string? sortBy = "firstName",
    string? sortDirection = "asc")
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var queryParams = new List<string>
    {
        $"pageNumber={pageNumber}",
        "pageSize=10",
        $"sortBy={sortBy}",
        $"sortDirection={sortDirection}"
    };

            if (!string.IsNullOrWhiteSpace(firstName))
                queryParams.Add($"firstName={Uri.EscapeDataString(firstName)}");

            if (!string.IsNullOrWhiteSpace(lastName))
                queryParams.Add($"lastName={Uri.EscapeDataString(lastName)}");

            if (!string.IsNullOrWhiteSpace(email))
                queryParams.Add($"email={Uri.EscapeDataString(email)}");

            if (!string.IsNullOrWhiteSpace(role))
                queryParams.Add($"role={Uri.EscapeDataString(role)}");

            var url = "api/Users?" + string.Join("&", queryParams);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error while loading users.";
                return View(new List<UserViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<List<UserViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            ViewBag.PageNumber = pageNumber;
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.Email = email;
            ViewBag.Role = role;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            return View(users ?? new List<UserViewModel>());
        }


        

        public IActionResult Create()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateAuthorizedClient();

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "api/Users",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Only Admin can create users.");

                TempData["Error"] = "Error while creating user.";

                return View(model);
            }

            TempData["Success"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/Users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (user == null)
            {
                TempData["Error"] = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/Users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (user == null)
            {
                TempData["Error"] = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            UserViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateAuthorizedClient();

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                $"api/Users/{id}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Only Admin can edit users.");

                TempData["Error"] = "Error while updating user.";

                return View(model);
            }

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/Users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<UserViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (user == null)
            {
                TempData["Error"] = "User not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.DeleteAsync(
                $"api/Users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error while deleting user.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}