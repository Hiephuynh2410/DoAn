using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;
        public ProductController()
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

            string uploadDirectory = @"C:\images";

            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Json("/images/" + fileName);
        }



        //Delete
        public async Task<IActionResult> Delete(int productId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ProductApi/delete/{productId}";

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
        public async Task<IActionResult> Detail(int productId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ProductApi/detail/{productId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ProductDetail = JsonConvert.DeserializeObject<Product>(responseContent);

                return View(ProductDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //search
        public async Task<IActionResult> SearchResult(string keyword)
        {
            List<Product> productList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if (string.IsNullOrEmpty(keyword))
                {
                    var response = await client.GetAsync("api/ProductApi");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        productList = JsonConvert.DeserializeObject<List<Product>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/ProductApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        productList = JsonConvert.DeserializeObject<List<Product>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
            }
            return View("Index", productList);
        }

        //edit
        [HttpGet]
        public IActionResult Edit(int productId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var product = db.Products.Find(productId);
            var ProductType = db.Producttypes.ToList();
            var providers = db.Providers.ToList();

            ViewBag.ProductType = new SelectList(ProductType, "ProductTypeId", "Name");
            ViewBag.providers = new SelectList(providers, "ProviderId", "Name");
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int productId, Product updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            var userIdSessionValue = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdSessionValue) && int.TryParse(userIdSessionValue, out int updatedByUserId))
            {
                updateModel.UpdatedBy = updatedByUserId;
            }
            var apiUrl = $"https://localhost:7109/api/ProductApi/update/{productId}";

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

        //create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var ProductType = db.Producttypes.ToList();
            var providers = db.Providers.ToList();

            ViewBag.ProductType = new SelectList(ProductType, "ProductTypeId", "Name");
            ViewBag.providers = new SelectList(providers, "ProviderId", "Name");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ProductApi/create";
            int createdByUserId;

            var userIdSessionValue = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userIdSessionValue) && int.TryParse(userIdSessionValue, out createdByUserId))
            {
                registrationModel.CreatedBy = createdByUserId;
            }
            if (db.Products.Any(p => p.Name == registrationModel.Name))
            {
                ModelState.AddModelError("Name", "Name already exists");
            }
            if (string.IsNullOrEmpty(registrationModel.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be empty");
            }
            if (string.IsNullOrEmpty(registrationModel.Description))
            {
                ModelState.AddModelError("Description", "Description cannot be empty");
            }
            if (string.IsNullOrEmpty(registrationModel.Price.ToString()))
            {
                ModelState.AddModelError("Price", "Price cannot be empty");
            }
            if (registrationModel.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be greater than 0");
            }
            if(string.IsNullOrEmpty(registrationModel.Quantity.ToString()))
            {
                ModelState.AddModelError("Quantity", "Quantity cannot be empty");
            }
            if (registrationModel.Quantity <= 0)
            {
                ModelState.AddModelError("Quantity", "Quantity must be greater than 0");
            }
            if (string.IsNullOrEmpty(Request.Form["ProductTypeId"]))
            {
                ModelState.AddModelError("ProductTypeId", "ProductTypeId is required.");
            }
            if (string.IsNullOrEmpty(Request.Form["ProviderId"]))
            {
                ModelState.AddModelError("ProviderId", "ProviderId is required.");
            }

            if (!ModelState.IsValid)
            {
                var ProductType = db.Producttypes.ToList();
                var providers = db.Providers.ToList();

                ViewBag.ProductType = new SelectList(ProductType, "ProductTypeId", "Name");
                ViewBag.providers = new SelectList(providers, "ProviderId", "Name");

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

                var ProductType = db.Producttypes.ToList();
                var providers = db.Providers.ToList();

                ViewBag.ProductType = new SelectList(ProductType, "ProductTypeId", "Name");
                ViewBag.providers = new SelectList(providers, "ProviderId", "Name");

                return View(registrationModel);
            }
        }


        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ProductApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<Product>>(responseContent);

                return View(products);
            }
            else
            {
                var productsList = await db.Products
                   .Include(s => s.ProductType)
                   .Include(s => s.Provider)
                   .ToListAsync();
                return View(productsList);
            }
        }
    }
}
