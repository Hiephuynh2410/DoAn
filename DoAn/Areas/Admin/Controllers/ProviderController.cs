using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

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

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Provider registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ProviderApi/create";

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

        //Delete
        public async Task<IActionResult> Delete(int providerId)
        {
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
