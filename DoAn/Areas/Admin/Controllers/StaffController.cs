using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;
        public StaffController()
        {
            _httpClient = new HttpClient();
        }
        //button choose image
        [HttpPost]
        public IActionResult ProcessUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json("No file uploaded");
            }

            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Json("/images/" + fileName);
        }
        //Login
        [HttpGet]
        public async Task<IActionResult> Login(Staff loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginModel);
            }

            var staff = await db.Staff.FirstOrDefaultAsync(c => c.Username == loginModel.Username);

            if (staff == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View("Login", loginModel);
            }

            var passwordHasher = new PasswordHasher<Staff>();
            var result = passwordHasher.VerifyHashedPassword(staff, staff.Password, loginModel.Password);

            if (result == PasswordVerificationResult.Success)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View("Login", loginModel);
        }

        //View List
        public async Task<IActionResult> Index()
        {
            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/AdminApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var staff = JsonConvert.DeserializeObject<List<Staff>>(responseContent);

                return View(staff);
            }
            else
            {
                return View();
            }
        }
        //register
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(Staff registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/AdminApi/register";

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
    }
}
