using FitnessRental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FitnessRental.Web.Controllers
{
    public class FitnessEquipmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FitnessEquipmentsController(IHttpClientFactory httpClientFactory)
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
    string? name,
    string? brand,
    string? equipmentType,
    string? isAvailable,
    int pageNumber = 1,
    string? sortBy = "name",
    string? sortDirection = "asc")
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = CreateAuthorizedClient();

            var queryParams = new List<string>
    {
        $"pageNumber={pageNumber}",
        "pageSize=10",
        $"sortBy={sortBy}",
        $"sortDirection={sortDirection}"
    };

            if (!string.IsNullOrWhiteSpace(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");

            if (!string.IsNullOrWhiteSpace(brand))
                queryParams.Add($"brand={Uri.EscapeDataString(brand)}");

            if (!string.IsNullOrWhiteSpace(equipmentType))
                queryParams.Add($"equipmentType={Uri.EscapeDataString(equipmentType)}");

            if (!string.IsNullOrWhiteSpace(isAvailable))
                queryParams.Add($"isAvailable={isAvailable}");

            var url = "api/FitnessEquipments?" + string.Join("&", queryParams);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error while loading equipment.";

                return View(new List<FitnessEquipmentViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var equipment = JsonSerializer.Deserialize<List<FitnessEquipmentViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            ViewBag.PageNumber = pageNumber;
            ViewBag.Name = name;
            ViewBag.Brand = brand;
            ViewBag.EquipmentType = equipmentType;
            ViewBag.IsAvailable = isAvailable;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            return View(equipment ?? new List<FitnessEquipmentViewModel>());
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
        public async Task<IActionResult> Create(FitnessEquipmentViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateAuthorizedClient();

            var imageUrl = await SaveImageAsync(model.ImageFile);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                model.ImageUrl = imageUrl;
            }

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "api/FitnessEquipments",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Only Admin can create equipment.");

                TempData["Error"] = "Error while creating equipment.";

                return View(model);
            }

            TempData["Success"] = "Equipment created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/FitnessEquipments/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Equipment not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var equipment = JsonSerializer.Deserialize<FitnessEquipmentViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (equipment == null)
            {
                TempData["Error"] = "Equipment not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(equipment);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/FitnessEquipments/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Equipment not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var equipment = JsonSerializer.Deserialize<FitnessEquipmentViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (equipment == null)
            {
                TempData["Error"] = "Equipment not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(equipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FitnessEquipmentViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateAuthorizedClient();

            var imageUrl = await SaveImageAsync(model.ImageFile);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                model.ImageUrl = imageUrl;
            }

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                $"api/FitnessEquipments/{id}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Only Admin can edit equipment.");

                TempData["Error"] = "Error while updating equipment.";

                return View(model);
            }

            TempData["Success"] = "Equipment updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "equipment");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/images/equipment/" + fileName;
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/FitnessEquipments/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Equipment not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var equipment = JsonSerializer.Deserialize<FitnessEquipmentViewModel>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (equipment == null)
            {
                TempData["Error"] = "Equipment not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(equipment);
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
                $"api/FitnessEquipments/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error while deleting equipment.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Equipment deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}