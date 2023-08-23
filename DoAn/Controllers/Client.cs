//using DoAn.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
//using System.Text;

//namespace DoAn.Controllers
//{
//    public class Client : Controller
//    {
//        private readonly HttpClient _httpClient;
//        DlctContext db = new DlctContext();

//        public Client()
//        {
//            _httpClient = new HttpClient();
//        }
//        //button choose image
//        [HttpPost]
//        public IActionResult ProcessUpload(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//            {
//                return Json("No file uploaded");
//            }

//            string fileName = Path.GetFileName(file.FileName);
//            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

//            using (var stream = new FileStream(filePath, FileMode.Create))
//            {
//                file.CopyTo(stream);
//            }

//            return Json("/images/" + fileName);
//        }

//        //Login
//        [HttpGet]
//        public async Task<IActionResult> Login(Cilent loginModel)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View("Login", loginModel);
//            }

//            var client = await db.Cilents.FirstOrDefaultAsync(c => c.Username == loginModel.Username);

//            if (client == null)
//            {
//                ModelState.AddModelError("", "Invalid username or password");
//                return View("Login", loginModel);
//            }

//            var passwordHasher = new PasswordHasher<Cilent>();
//            var result = passwordHasher.VerifyHashedPassword(client, client.Password, loginModel.Password);

//            if (result == PasswordVerificationResult.Success)
//            {
//                return RedirectToAction("Index");
//            }

//            ModelState.AddModelError("", "Invalid username or password");
//            return View("Login", loginModel);
//        }

//        //View List
//        public async Task<IActionResult> Index()
//        {
//            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ClientLogin/");
//            if (apiResponse.IsSuccessStatusCode)
//            {
//                var responseContent = await apiResponse.Content.ReadAsStringAsync();
//                var clients = JsonConvert.DeserializeObject<List<Cilent>>(responseContent);

//                return View(clients);
//            }
//            else
//            {
//                var clients = await db.Cilents
//                  .Include(s => s.Role)
//                  .ToListAsync();
//                return View(clients);
//            }
//        }

//        //register
//        public IActionResult Register()
//        {
//            return View();
//        }
//        [HttpPost]
//        public async Task<IActionResult> Register(Cilent registrationModel)
//        {
//            var apiUrl = "https://localhost:7109/api/ClientLogin/Register";

//            var json = JsonConvert.SerializeObject(registrationModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PostAsync(apiUrl, content);

//            if (response.IsSuccessStatusCode)
//            {
//                return RedirectToAction("Index");
//            }
//            else
//            {
//                var responseContent = await response.Content.ReadAsStringAsync();
//                Console.WriteLine("API Response Content: " + responseContent);


//                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

//                ModelState.AddModelError("", errorResponse.ToString());
//                return View(registrationModel);
//            }
//        }

//        //Delete
//        public async Task<IActionResult> Delete(int clientId)
//        {
//            var apiUrl = $"https://localhost:7109/api/ClientLogin/delete/{clientId}";

//            var response = await _httpClient.DeleteAsync(apiUrl);

//            if (response.IsSuccessStatusCode)
//            {
//                return RedirectToAction("Index"); // Redirect to the list of clients after successful deletion
//            }
//            else
//            {
//                var responseContent = await response.Content.ReadAsStringAsync();
//                Console.WriteLine("API Response Content: " + responseContent);

//                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

//                ModelState.AddModelError("", errorResponse.ToString());
//                return RedirectToAction("Index"); // You can choose to handle the error scenario differently
//            }
//        }

//        //edit
//        [HttpGet]
//        public IActionResult Edit(int clientId)
//        {
//            var client = db.Cilents.Find(clientId);
//            if(client == null) {
//                return NotFound();  
//            }
//            return View(client);
//        }
//        [HttpPost]
//        public async Task<IActionResult> Edit(int clientId, Cilent updateModel)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(updateModel);
//            }
//            var apiUrl = $"https://localhost:7109/api/ClientLogin/update/{clientId}";

//            var json = JsonConvert.SerializeObject(updateModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _httpClient.PutAsync(apiUrl, content);

//            if (response.IsSuccessStatusCode)
//            {
//                return RedirectToAction("Index");
//            }
//            else
//            {
//                var responseContent = await response.Content.ReadAsStringAsync();
//                Console.WriteLine("API Response Content: " + responseContent);

//                var errorResponse = JsonConvert.DeserializeObject<object>(responseContent);

//                ModelState.AddModelError("", errorResponse.ToString());
//                return View(updateModel);
//            }
//        }

//        //detail
//        [HttpGet]
//        public async Task<IActionResult> Detail(int clientId)
//        {
//            var apiUrl = $"https://localhost:7109/api/ClientLogin/detail/{clientId}";

//            var apiResponse = await _httpClient.GetAsync(apiUrl);
//            if (apiResponse.IsSuccessStatusCode)
//            {
//                var responseContent = await apiResponse.Content.ReadAsStringAsync();
//                var clientDetail = JsonConvert.DeserializeObject<Cilent>(responseContent);

//                return View(clientDetail);
//            }
//            else
//            {
//                // Handle the error scenario
//                return RedirectToAction("Index"); // You can choose to handle the error differently
//            }
//        }
//    }
//}
