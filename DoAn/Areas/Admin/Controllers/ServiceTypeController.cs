using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceTypeController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public ServiceTypeController()
        {
            _httpClient = new HttpClient();
        }

        //Delete
        public async Task<IActionResult> Delete(int Servicetypeid)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ServicesTypeApi/delete/{Servicetypeid}";

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
        public async Task<IActionResult> Detail(int Servicetypeid)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ServicesTypeApi/detail/{Servicetypeid}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ServicetypeidDetail = JsonConvert.DeserializeObject<Servicetype>(responseContent);

                return View(ServicetypeidDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        //edit
        [HttpGet]
        public IActionResult Edit(int Servicetypeid)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Staff");
            }

            var Servicetype = db.Servicetypes.Find(Servicetypeid);
            if (Servicetype == null)
            {
                return NotFound();
            }
            return View(Servicetype);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int Servicetypeid, Servicetype updateModel)
        {
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }
           
            var apiUrl = $"https://localhost:7109/api/ServicesTypeApi/update/{Servicetypeid}";

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
        public async Task<IActionResult> Create(Servicetype registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ServicesTypeApi/create";
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

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ServicesTypeApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ServicesType = JsonConvert.DeserializeObject<List<Servicetype>>(responseContent);

                return View(ServicesType);
            }
            else
            {
                var ServicesType = await db.Servicetypes
                   .ToListAsync();
                return View(ServicesType);
            }
        }
    }
}
