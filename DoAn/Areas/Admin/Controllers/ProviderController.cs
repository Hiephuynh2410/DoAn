using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProviderController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public ProviderController()
        {   
            _httpClient = new HttpClient();
        }

        //Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Provider registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ProviderApi/create";
            var checkProvider = db.Providers.FirstOrDefault(x => x.Name == registrationModel.Name);

            if (checkProvider != null)
            {
                ModelState.AddModelError("Name", "Name with this name already exists.");
                return View(registrationModel);
            }

            if (string.IsNullOrEmpty(registrationModel.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be empty");
            }

            if (string.IsNullOrEmpty(registrationModel.Address))
            {
                ModelState.AddModelError("Address", "Address cannot be empty");
            }

            if (string.IsNullOrEmpty(registrationModel.Phone))
            {
                ModelState.AddModelError("Phone", "Phone cannot be empty");
            }

            if (string.IsNullOrEmpty(registrationModel.Email))
            {
                ModelState.AddModelError("Email", "Email cannot be empty");
            }
            else
            {
                var emailRegex = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
                if (!Regex.IsMatch(registrationModel.Email, emailRegex))
                {
                    ModelState.AddModelError("Email", "sai định dạng email");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(registrationModel);
            }

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

                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);
                ModelState.AddModelError("", errorResponse.ToString());
                return View(registrationModel);
            }
        }


        //search
        public async Task<IActionResult> SearchResult(string keyword)
        {
            List<Provider> providersList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if (string.IsNullOrEmpty(keyword))
                {
                    var response = await client.GetAsync("api/ProviderApi");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        providersList = JsonConvert.DeserializeObject<List<Provider>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/ProviderApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        providersList = JsonConvert.DeserializeObject<List<Provider>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                return View("Index", providersList);
            }
        }

        //Delete
        public async Task<IActionResult> Delete(int providerId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ProviderApi/delete/{providerId}";

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

        //edit
        [HttpGet]
        public IActionResult Edit(int providerId)
        {
            var provider = db.Providers.Find(providerId);
            if (provider == null)
            {
                return NotFound();
            }
            return View(provider);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int providerId, Provider updateModel)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            var apiUrl = $"https://localhost:7109/api/ProviderApi/update/{providerId}";

            var json = JsonConvert.SerializeObject(updateModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

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
                return View(updateModel);
            }
        }

        //detail
        [HttpGet]
        public async Task<IActionResult> Detail(int providerId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ProviderApi/detail/{providerId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ProviderDetail = JsonConvert.DeserializeObject<Provider>(responseContent);

                return View(ProviderDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ProviderApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var provider = JsonConvert.DeserializeObject<List<Provider>>(responseContent);

                return View(provider);
            }
            else
            {
                var provider = await db.Providers
                   .ToListAsync();
                return View(provider);
            }
        }
    }
}
