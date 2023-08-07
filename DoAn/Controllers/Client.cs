using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Controllers
{
    public class Client : Controller
    {

        private readonly HttpClient _httpClient;

        public Client()
        {
            _httpClient = new HttpClient();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Cilent registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ClientLogin/Register";  // Update with your API URL

            var json = JsonConvert.SerializeObject(registrationModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Registration successful, you can redirect or show a success message
                return RedirectToAction("Index");
            }
            else
            {
                // Handle registration failure, display error message or validation errors
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

                ModelState.AddModelError("", errorResponse.ToString());
                return View(registrationModel);
            }
        }

        public async Task<IActionResult> Index()
        {
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ClientLogin/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Cilent>>(responseContent);

                return View(clients);
            }
            else
            {
                // Handle API error
                return View(); // Return an empty view or an error view
            }
        }
        
    }
}
