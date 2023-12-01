using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogCategoryController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public BlogCategoryController()
        {
            _httpClient = new HttpClient();
        }

        //Delete 
        public async Task<IActionResult> Delete(int BlogCategoryId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/BlogCategoryApi/delete/{BlogCategoryId}";

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
        public async Task<IActionResult> Detail(int BlogCategoryId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/BlogCategoryApi/detail/{BlogCategoryId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var BlogCategoryDetail = JsonConvert.DeserializeObject<BlogCategory>(responseContent);

                return View(BlogCategoryDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //edit
        [HttpGet]
        public IActionResult Edit(int BlogCategoryId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var BlogCategorys = db.BlogCategories.Find(BlogCategoryId);
            if (BlogCategorys == null)
            {
                return NotFound();
            }
            return View(BlogCategorys);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int BlogCategoryId, BlogCategory updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
         
            var apiUrl = $"https://localhost:7109/api/BlogCategoryApi/update/{BlogCategoryId}";

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
        public async Task<IActionResult> Create(BlogCategory registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/BlogCategoryApi/create";
            int createdByUserId;
            var checkBlogCatetory = db.BlogCategories.FirstOrDefault(x => x.Title ==  registrationModel.Title); 
            if (checkBlogCatetory != null)
            {
                ModelState.AddModelError("Title", "Title đã tồn tại vui lòng thêm title khác");
                return View(registrationModel);
            }
            if(string.IsNullOrEmpty(registrationModel.Title))
            {
                ModelState.AddModelError("Title", "Title cannot be empty");
            }
            if (string.IsNullOrEmpty(registrationModel.Description))
            {
                ModelState.AddModelError("Description", "Description cannot be empty");
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

        //view
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/BlogCategoryApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var BlogCategorys = JsonConvert.DeserializeObject<List<BlogCategory>>(responseContent);

                return View(BlogCategorys);
            }
            else
            {
                var BlogCategorys = await db.BlogCategories
                   .ToListAsync();
                return View(BlogCategorys);
            }
        }
    }
}
