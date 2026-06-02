using FitnessRental.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FitnessRental.Web.Controllers
{
    public class EquipmentRentalsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EquipmentRentalsController(
            IHttpClientFactory httpClientFactory)
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

        private async Task CalculateTotalPrice(
            EquipmentRentalViewModel model)
        {
            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                $"api/FitnessEquipments/{model.FitnessEquipmentId}");

            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync();

            var equipment =
                JsonSerializer.Deserialize<FitnessEquipmentViewModel>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (equipment == null)
                return;
            var totalDays =
                   (model.ReturnDate -  model.RentDate).Value.TotalDays;

            if (totalDays <= 0)
            {
                totalDays = 1;
            }

            model.TotalPrice =
                (decimal)totalDays *
                equipment.RentalPricePerDay;
        }

        private async Task LoadEquipmentDropdown()
        {
            var client = CreateAuthorizedClient();

            var response = await client.GetAsync(
                "api/FitnessEquipments?pageNumber=1&pageSize=100");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Equipments = new List<SelectListItem>();
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            var equipments =
                JsonSerializer.Deserialize<List<FitnessEquipmentViewModel>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            ViewBag.Equipments = equipments!
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} - {e.Brand}"
                })
                .ToList();
        }

        public async Task<IActionResult> Index(
                    string? userEmail,
                    string? equipmentName,
                    string? status,
                    int pageNumber = 1,
                    string? sortBy = "rentDate",
                    string? sortDirection = "desc")
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = CreateAuthorizedClient();

            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            var queryParams = new List<string>
            {
               $"pageNumber={pageNumber}",
               "pageSize=10",
               $"sortBy={sortBy}",
               $"sortDirection={sortDirection}"
            };

            if (role != "Admin" && userId.HasValue)
                queryParams.Add($"userId={userId.Value}");

            if (!string.IsNullOrWhiteSpace(userEmail))
                queryParams.Add($"userEmail={Uri.EscapeDataString(userEmail)}");

            if (!string.IsNullOrWhiteSpace(equipmentName))
                queryParams.Add($"equipmentName={Uri.EscapeDataString(equipmentName)}");

            if (!string.IsNullOrWhiteSpace(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");

            var url = "api/EquipmentRentals?" + string.Join("&", queryParams);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error while loading rentals.";
                return View(new List<EquipmentRentalViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var rentals = JsonSerializer.Deserialize<List<EquipmentRentalViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            ViewBag.PageNumber = pageNumber;
            ViewBag.UserEmail = userEmail;
            ViewBag.EquipmentName = equipmentName;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            return View(rentals ?? new List<EquipmentRentalViewModel>());
        }


        public async Task<IActionResult> Create(int? fitnessEquipmentId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var model = new EquipmentRentalViewModel
            {
                RentDate = DateTime.Now,
                ReturnDate = DateTime.Now.AddDays(7),
                TotalPrice = 0,
                Status = "Active"
            };

            model.UserId =
                HttpContext.Session.GetInt32("UserId") ?? 0;

            if (fitnessEquipmentId.HasValue)
            {
                model.FitnessEquipmentId =
                    fitnessEquipmentId.Value;
            }

            await LoadEquipmentDropdown();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
                    EquipmentRentalViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            model.UserId =
                HttpContext.Session.GetInt32("UserId") ?? 0;

            model.Status = "Active";

            if (model.RentDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "RentDate",
                    "Rent date cannot be in the future.");

                await LoadEquipmentDropdown();

                return View(model);
            }

            if (model.ReturnDate < model.RentDate)
            {
                ModelState.AddModelError(
                    "ReturnDate",
                    "Return date cannot be before rent date.");

                await LoadEquipmentDropdown();

                return View(model);
            }

            await CalculateTotalPrice(model);

            if (!ModelState.IsValid)
            {
                await LoadEquipmentDropdown();
                return View(model);
            }

            var client = CreateAuthorizedClient();

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                "api/EquipmentRentals",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Error while creating rental.");

                TempData["Error"] = "Error while creating rental.";

                await LoadEquipmentDropdown();

                return View(model);
            }

            TempData["Success"] = "Rental created successfully.";

            return RedirectToAction(nameof(Index));
        }


        ////    [HttpPost]
        ////    [ValidateAntiForgeryToken]
        //////    public async Task<IActionResult> Create(
        //////EquipmentRentalViewModel model)
        //////    {
        //////        if (!IsLoggedIn())
        //////            return RedirectToAction("Login", "Account");

        //////        if (IsAdmin())
        //////            return RedirectToAction("AccessDenied", "Account");

        //////        model.UserId =
        //////            HttpContext.Session.GetInt32("UserId") ?? 0;

        //////        model.Status = "Active";

        //////        await CalculateTotalPrice(model);

        //////        if (!ModelState.IsValid)
        //////        {
        //////            await LoadEquipmentDropdown();
        //////            return View(model);
        //////        }

        //////        var client = CreateAuthorizedClient();

        //////        var json = JsonSerializer.Serialize(model);

        //////        var content = new StringContent(
        //////            json,
        //////            Encoding.UTF8,
        //////            "application/json");

        //////        var response = await client.PostAsync(
        //////            "api/EquipmentRentals",
        //////            content);

        //////        if (!response.IsSuccessStatusCode)
        //////        {
        //////            ModelState.AddModelError(
        //////                "",
        //////                "Error while creating rental.");

        //////            TempData["Error"] = "Error while creating rental.";

        //////            await LoadEquipmentDropdown();

        //////            return View(model);
        //////        }

        //////        TempData["Success"] = "Rental created successfully.";

        //////        return RedirectToAction(nameof(Index));
        //////    }
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = CreateAuthorizedClient();

            var response =
                await client.GetAsync($"api/EquipmentRentals/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Rental not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var rental =
                JsonSerializer.Deserialize<EquipmentRentalViewModel>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (rental == null)
            {
                TempData["Error"] = "Rental not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(rental);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = CreateAuthorizedClient();

            var response =
                await client.GetAsync($"api/EquipmentRentals/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Rental not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var rental =
                JsonSerializer.Deserialize<EquipmentRentalViewModel>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (rental == null)
            {
                TempData["Error"] = "Rental not found.";

                return RedirectToAction(nameof(Index));
            }

            await LoadEquipmentDropdown();

            return View(rental);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EquipmentRentalViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            await CalculateTotalPrice(model);

            if (!ModelState.IsValid)
            {
                await LoadEquipmentDropdown();
                return View(model);
            }

            var client = CreateAuthorizedClient();

            var json = JsonSerializer.Serialize(model);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                $"api/EquipmentRentals/{id}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Error while updating rental.");

                TempData["Error"] = "Error while updating rental.";

                await LoadEquipmentDropdown();

                return View(model);
            }

            TempData["Success"] = "Rental updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!IsAdmin())
                return RedirectToAction("AccessDenied", "Account");

            var client = CreateAuthorizedClient();

            var response =
                await client.GetAsync($"api/EquipmentRentals/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Rental not found.";

                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var rental =
                JsonSerializer.Deserialize<EquipmentRentalViewModel>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (rental == null)
            {
                TempData["Error"] = "Rental not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(rental);
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

            var response =
                await client.DeleteAsync($"api/EquipmentRentals/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error while deleting rental.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Rental deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}




