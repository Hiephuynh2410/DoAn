using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;
using NuGet.Protocol;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;
        public ServiceController()
        {
            _httpClient = new HttpClient();
        }

        //View List
        public async Task<IActionResult> Index()
        {
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/ServiceApi/AdminApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var services = JsonConvert.DeserializeObject<List<Service>>(responseContent);
                return View(services);
            }
            else
            {
                var servicesList = await db.Services
                    .Include(s => s.ServiceType)
                    .ToListAsync();
                return View(servicesList);
            }
        }
        //create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var serviceTypes = db.Servicetypes.ToList();
            ViewBag.serviceTypes = new SelectList(serviceTypes, "ServiceTypeId", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ServiceApi/create";
            if (string.IsNullOrEmpty(registrationModel.Name) && string.IsNullOrEmpty(registrationModel.Price.ToString()))
            {
                ModelState.AddModelError("Name", "cannot be empty.");
                ModelState.AddModelError("Price", "cannot be empty.");
            }
            if (registrationModel.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be greater than 0.");
            }

            if (string.IsNullOrEmpty(Request.Form["ServiceTypeId"]))
            {
                ModelState.AddModelError("ServiceTypeId", "ServiceType is required.");
            }
            if (ModelState.IsValid)
            {
                int createdByUserId;
                if (int.TryParse(HttpContext.Session.GetString("UserId"), out createdByUserId))
                {
                    registrationModel.CreatedBy = createdByUserId;
                }
                registrationModel.Status = Request.Form["Status"] == "true";
                var json = JsonConvert.SerializeObject(registrationModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("API Response Content: " + responseContent);

                    dynamic errorResponse = JsonConvert.DeserializeObject(responseContent);

                    if (errorResponse != null)
                    {
                        if (!string.IsNullOrEmpty(errorResponse.Message))
                        {
                            ModelState.AddModelError("", errorResponse.Message);
                        }
                        if (errorResponse.Errors != null)
                        {
                            foreach (var error in errorResponse.Errors)
                            {
                                ModelState.AddModelError("", error.ToString());
                            }
                        }
                    }

                    var serviceTypes = db.Servicetypes.ToList();
                    ViewBag.serviceTypes = new SelectList(serviceTypes, "ServiceTypeId", "Name");

                    return View(registrationModel);
                }
            }
            else
            {
                var serviceTypes = db.Servicetypes.ToList();
                ViewBag.serviceTypes = new SelectList(serviceTypes, "ServiceTypeId", "Name");
                return View(registrationModel);
            }
        }

        //edit
        [HttpGet]
        public IActionResult Edit(int serviceId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var service = db.Services
              .Include(s => s.ServiceType)
              .FirstOrDefault(s => s.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound();
            }

            var serviceTypes = db.Servicetypes.ToList();
            ViewBag.serviceTypes = new SelectList(serviceTypes, "ServiceTypeId", "Name");

            return View(service);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int serviceId, Service updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            updateModel.Status = Request.Form["Status"] == "true";

            var apiUrl = $"https://localhost:7109/api/ServiceApi/update/{serviceId}";

            var json = JsonConvert.SerializeObject(updateModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var updatedService = await db.Services.FirstOrDefaultAsync(s => s.ServiceId == serviceId);
                    if (updatedService != null)
                    {
                        int editorId;
                        if (int.TryParse(HttpContext.Session.GetString("UserId"), out editorId))
                        {
                            updatedService.UpdatedBy = editorId;
                        }
                        db.Entry(updatedService).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the editor's name: " + ex.Message);
                }
                return RedirectToAction("Index");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return View(updateModel);
            }
        }

        //Delete
        public async Task<IActionResult> Delete(int serviceId)
        {
            var apiUrl = $"https://localhost:7109/api/ServiceApi/delete/{serviceId}";

            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("API Response Content: " + responseContent);

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return RedirectToAction("Index");
            }
        }

        //detail
        [HttpGet]
        public async Task<IActionResult> Detail(int serviceId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ServiceApi/detail/{serviceId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ServiceDetail = JsonConvert.DeserializeObject<Service>(responseContent);
                return View(ServiceDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //search
        public async Task<IActionResult> SearchResult(string keyword)
        {
            List<Service> servicesList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if (string.IsNullOrEmpty(keyword))
                {
                    var response = await client.GetAsync("api/ServiceApi");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        servicesList = JsonConvert.DeserializeObject<List<Service>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/ServiceApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        servicesList = JsonConvert.DeserializeObject<List<Service>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                return View("Index", servicesList);

            }
        }
    }
}
