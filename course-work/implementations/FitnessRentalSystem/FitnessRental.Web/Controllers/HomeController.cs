using FitnessRental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FitnessRental.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("UserRole");

            var client = CreateAuthorizedClient();

            var model = new DashboardViewModel
            {
                EquipmentCount = await GetCountAsync<FitnessEquipmentViewModel>(
                    client,
                    "api/FitnessEquipments?pageNumber=1&pageSize=1000"),

                RentalsCount = await GetCountAsync<EquipmentRentalViewModel>(
                    client,
                    "api/EquipmentRentals?pageNumber=1&pageSize=1000")
            };

            if (role == "Admin")
            {
                model.UsersCount = await GetCountAsync<UserViewModel>(
                    client,
                    "api/Users?pageNumber=1&pageSize=1000");
            }

            return View(model);
        }

        private async Task<int> GetCountAsync<T>(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return 0;

            var json = await response.Content.ReadAsStringAsync();

            var items = JsonSerializer.Deserialize<List<T>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return items?.Count ?? 0;
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}