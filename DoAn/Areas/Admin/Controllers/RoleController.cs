using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public RoleController()
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
        public async Task<IActionResult> Create(Role registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/RoleApi/create";

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
        //edit
        [HttpGet]
        public IActionResult Edit(int RoleId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var Role = db.Roles.Find(RoleId);
            if (Role == null)
            {
                return NotFound();
            }
            return View(Role);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int RoleId, Role updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
           
            var apiUrl = $"https://localhost:7109/api/RoleApi/update/{RoleId}";

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

        //Delete
        public async Task<IActionResult> Delete(int RoleId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/RoleApi/delete/{RoleId}";

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
        public async Task<IActionResult> Detail(int RoleId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/RoleApi/detail/{RoleId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var roleDetail = JsonConvert.DeserializeObject<Role>(responseContent);

                return View(roleDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/RoleApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var Role = JsonConvert.DeserializeObject<List<Role>>(responseContent);

                return View(Role);
            }
            else
            {
                var Role = await db.Roles
                   .ToListAsync();
                return View(Role);
            }
        }
    }
}
