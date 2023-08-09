using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BranchController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;
        public BranchController()
        {
            _httpClient = new HttpClient();

        }
        //Create
        public IActionResult Create()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Branch registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/BranchApi/create";

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

        //View List
        public async Task<IActionResult> Index()
        {
            
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/BranchApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var branch = JsonConvert.DeserializeObject<List<Branch>>(responseContent);

                return View(branch);
            }
            else
            {
                return View();
            }
        }
    }
}
