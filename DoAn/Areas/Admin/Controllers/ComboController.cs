using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ComboController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public ComboController()
        {
            _httpClient = new HttpClient();
        }
        //search
        public async Task<IActionResult> SearchResult(string keyword)
        {
            List<Combo> comboList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if (string.IsNullOrEmpty(keyword))
                {
                    var response = await client.GetAsync("api/ComboApi");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        comboList = JsonConvert.DeserializeObject<List<Combo>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/ComboApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        comboList = JsonConvert.DeserializeObject<List<Combo>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
            }
            return View("Index", comboList);
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
        public async Task<IActionResult> Create(Combo registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ComboApi/create";
            int createdByUserId;

            var userIdSessionValue = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userIdSessionValue) && int.TryParse(userIdSessionValue, out createdByUserId))
            {
                registrationModel.CreatedBy = createdByUserId;
            }
            var ComboName = registrationModel.Name?.Trim();

            var checkCombo = db.Combos.FirstOrDefault(x => x.Name == ComboName);
            if (checkCombo != null)
            {
                ModelState.AddModelError("Name", "Combo with this name already exists.");
                return View(registrationModel);
            }
            if (string.IsNullOrEmpty(registrationModel.Name))
            {
                ModelState.AddModelError("Name", "Name cannot be empty.");
            }

            if (string.IsNullOrEmpty(registrationModel.Price.ToString()) || registrationModel.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price cannot be empty or bé hơn 0");
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
        public async Task<IActionResult> Delete(int comboId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ComboApi/delete/{comboId}";

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
        public IActionResult Edit(int comboId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var combo = db.Combos.Find(comboId);
            if (combo == null)
            {
                return NotFound();
            }
            return View(combo);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int comboId, Combo updateModel)
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
            var apiUrl = $"https://localhost:7109/api/ComboApi/update/{comboId}";

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
        public async Task<IActionResult> Detail(int comboId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ComboApi/detail/{comboId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ComboDetail = JsonConvert.DeserializeObject<Combo>(responseContent);

                return View(ComboDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ComboApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var combo = JsonConvert.DeserializeObject<List<Combo>>(responseContent);

                return View(combo);
            }
            else
            {
                var combo = await db.Combos
                   .ToListAsync();
                return View(combo);
            }
        }
    }
}
