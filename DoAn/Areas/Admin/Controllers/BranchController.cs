using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
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
        //search
        public async Task<IActionResult> SearchResult(string keyword)
        {
            List<Branch> branchList;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7109/");
                if (string.IsNullOrEmpty(keyword))
                {
                    var response = await client.GetAsync("api/BranchApi");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        branchList = JsonConvert.DeserializeObject<List<Branch>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
                else
                {
                    var response = await client.GetAsync($"api/BranchApi/search?keyword={keyword}");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        branchList = JsonConvert.DeserializeObject<List<Branch>>(responseContent);
                    }
                    else
                    {
                        return View("Index");
                    }
                }
            }
            return View("Index", branchList);
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

            var checkBranch = db.Branches.FirstOrDefault(b => b.Address == registrationModel.Address);
            if(checkBranch != null)
            {
                ModelState.AddModelError("Address", "Address with this name already exists.");
                return View(registrationModel);
            }
            if (string.IsNullOrEmpty(registrationModel.Address))
            {
                ModelState.AddModelError("Address", "Address cannot be empty.");
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

        //edit
        [HttpGet]
        public IActionResult Edit(int branchId)
        {
            var branch = db.Branches.Find(branchId);
            if (branch == null)
            {
                return NotFound();
            }
            return View(branch);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int branchId, Branch updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
            var apiUrl = $"https://localhost:7109/api/BranchApi/update/{branchId}";

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
        public async Task<IActionResult> Delete(int branchId)
        {
            var apiUrl = $"https://localhost:7109/api/BranchApi/delete/{branchId}";

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
        public async Task<IActionResult> Detail(int branchId)
        {
            var apiUrl = $"https://localhost:7109/api/BranchApi/detail/{branchId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var BranchDetail = JsonConvert.DeserializeObject<Branch>(responseContent);

                return View(BranchDetail);
            }
            else
            {
                return RedirectToAction("Index");
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
                var branch = await db.Branches
                   .Include(s => s.Staff)
                   .ToListAsync();
                return View(branch);
            }
        }
    }
}
