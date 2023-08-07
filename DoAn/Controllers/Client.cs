using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DoAn.Controllers
{
    public class Client : Controller
    {

        private readonly HttpClient _httpClient;

        public Client()
        {
            _httpClient = new HttpClient();
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
