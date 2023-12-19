using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ScheduleController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public ScheduleController()
        {
            _httpClient = new HttpClient();
        }

        //Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Create", "Schedule"));

                return RedirectToAction("Login", "Staff");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Schedule registrationModel)
        {
            var apiUrl = "https://localhost:7109/api/ScheduleApi/create";


            var existingTime = await db.Schedules.AnyAsync(s => s.Time == registrationModel.Time);

            if (registrationModel.Time == null ||
                registrationModel.Time.Value < TimeSpan.Zero ||
                registrationModel.Time.Value >= TimeSpan.FromDays(1))
            {
                ModelState.AddModelError("Time", "Time must be between 00:00:00.0000000 and 23:59:59.9999999.");
            }
            else if (existingTime)
            {
                ModelState.AddModelError("Time", "Time already exists in the database.");
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
        public async Task<IActionResult> Delete(int scheduleId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Delete", "Schedule", new {scheduleId}));

                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ScheduleApi/delete/{scheduleId}";

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
        public IActionResult Edit(int scheduleId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Edit", "Schedule"));

                return RedirectToAction("Login", "Staff");
            }

            var schedule = db.Schedules.Find(scheduleId);
            if (schedule == null)
            {
                return NotFound();
            }
            return View(schedule);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int scheduleId, Schedule updateModel)
        {


            if (updateModel.Time == null ||
                updateModel.Time.Value < TimeSpan.Zero ||
                updateModel.Time.Value >= TimeSpan.FromDays(1))
            {
                ModelState.AddModelError("Time", "Time must be between 00:00:00.0000000 and 23:59:59.9999999.");
            }
            if (!ModelState.IsValid)
            {
                return View(updateModel);
            }

            var apiUrl = $"https://localhost:7109/api/ScheduleApi/update/{scheduleId}";

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
        public async Task<IActionResult> Detail(int scheduleId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                HttpContext.Session.SetString("ReturnUrl", Url.Action("Detail", "Schedule", new {scheduleId}));

                return RedirectToAction("Login", "Staff");
            }
            var apiUrl = $"https://localhost:7109/api/ScheduleApi/detail/{scheduleId}";

            var apiResponse = await _httpClient.GetAsync(apiUrl);
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var ScheduleDetail = JsonConvert.DeserializeObject<Schedule>(responseContent);

                return View(ScheduleDetail);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ScheduleApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var schedules = JsonConvert.DeserializeObject<List<Schedule>>(responseContent);

                return View(schedules);
            }
            else
            {
                var schedules = await db.Schedules
                     .Include(s => s.Scheduledetails)
                   .ToListAsync();
                return View(schedules);
            }
        }
    }
}
