using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ScheduleDetailController : Controller
    {
        DlctContext db = new DlctContext();
        private readonly HttpClient _httpClient;

        public ScheduleDetailController()
        {
            _httpClient = new HttpClient();
        }

        //View List
        public async Task<IActionResult> Index()
        {

            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ScheduleDetailApi/");
            if (apiResponse.IsSuccessStatusCode)
            {
                var responseContent = await apiResponse.Content.ReadAsStringAsync();
                var schedules = JsonConvert.DeserializeObject<List<Scheduledetail>>(responseContent);
                return View(schedules);
            }
            else
            {
                var schedules = await db.Scheduledetails
                 .ToListAsync();
                return View(schedules);
            }
        }
    }
}
