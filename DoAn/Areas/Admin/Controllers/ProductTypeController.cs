using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductTypeController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public ProductTypeController()
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
        public async Task<IActionResult> Create(Producttype registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ProductTypeApi/create";
            var checkProductTupe = db.Producttypes.FirstOrDefault(x => x.Name == registrationModel.Name); 
            if(checkProductTupe != null)
            {
                ModelState.AddModelError("Name", "Name tồn tại rồi đừng thêm nữa");
                return View(registrationModel);
            }
            if(string.IsNullOrEmpty(registrationModel.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be empty");
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

        //Delete
        public async Task<IActionResult> Delete(int ProductTypeId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ProductTypeApi/delete/{ProductTypeId}";

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
        public IActionResult Edit(int ProductTypeId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var producttype = db.Producttypes.Find(ProductTypeId);
            if (producttype == null)
            {
                return NotFound();
            }
            return View(producttype);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int ProductTypeId, Producttype updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            
            var apiUrl = $"https://localhost:7109/api/ProductTypeApi/update/{ProductTypeId}";

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
        public async Task<IActionResult> Detail(int ProductTypeId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ProductTypeApi/detail/{ProductTypeId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ProductTypeDetail = JsonConvert.DeserializeObject<Producttype>(responseContent);

                return View(ProductTypeDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ProductTypeApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ProductType = JsonConvert.DeserializeObject<List<Producttype>>(responseContent);

                return View(ProductType);
            }
            else
            {
                var ProductType = await db.Producttypes
                   .ToListAsync();
                return View(ProductType);
            }
        }

        //search
        public async Task<IActionResult> SearchResult(string keyword)
        {
            List<Producttype> producttypesList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if(string.IsNullOrEmpty(keyword))
                {
                    var response = await client.GetAsync("api/ProductTypeApi");
                    if(response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        producttypesList = JsonConvert.DeserializeObject<List<Producttype>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/ProductTypeApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        producttypesList = JsonConvert.DeserializeObject<List<Producttype>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                return View("Index", producttypesList);

            }
        }
    }
}
