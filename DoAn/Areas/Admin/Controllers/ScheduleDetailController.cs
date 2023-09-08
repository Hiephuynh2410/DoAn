using DoAn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

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

        //public async Task<IActionResult> Index()
        //{
        //    // kiểm tra session 
        //    if (HttpContext.Session.TryGetValue("UserId", out byte[] userIdBytes))
        //    {
        //        if (int.TryParse(Encoding.UTF8.GetString(userIdBytes), out int staffId))
        //        {
        //            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ScheduleDetailApi/");
        //            if (apiResponse.IsSuccessStatusCode)
        //            {
        //                var responseContent = await apiResponse.Content.ReadAsStringAsync();
        //                var schedules = JsonConvert.DeserializeObject<List<Scheduledetail>>(responseContent);

        //                //lọc lịch làm việc dựa theo StaffId
        //                var filteredSchedules = schedules.Where(s => s.Staff.StaffId == staffId).ToList();

        //                return View(filteredSchedules);
        //            }
        //            else
        //            {
        //                var schedules = await db.Scheduledetails
        //                    .Where(s => s.Staff.StaffId == staffId)
        //                    .ToListAsync();

        //                return View(schedules);
        //            }
        //        }
        //    }

        //    //nếu nhân viên không có lịch làm việc thì thôi không hiện
        //    return View(new List<Scheduledetail>());
        //}
        //public async Task<IActionResult> Index()
        //{
        //    if (HttpContext.Session.TryGetValue("UserId", out byte[] userIdBytes))
        //    {
        //        if (int.TryParse(Encoding.UTF8.GetString(userIdBytes), out int staffId))
        //        {
        //            var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ScheduleDetailApi/");
        //            if (apiResponse.IsSuccessStatusCode)
        //            {
        //                var responseContent = await apiResponse.Content.ReadAsStringAsync();
        //                var schedules = JsonConvert.DeserializeObject<List<Scheduledetail>>(responseContent);

        //                var filteredSchedules = schedules.Where(s => s.Staff.StaffId == staffId).ToList();

        //                return View(filteredSchedules);
        //            }
        //            else
        //            {
        //                var schedules = await db.Scheduledetails
        //                    .Where(s => s.Staff.StaffId == staffId)
        //                    .ToListAsync();

        //                return View(schedules);
        //            }
        //        }
        //    }

        //    ViewBag.LoginMessage = "Please log in to view the schedule.";
        //    return View(); 
        //}
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.TryGetValue("UserId", out byte[] userIdBytes))
            {
                if (int.TryParse(Encoding.UTF8.GetString(userIdBytes), out int staffId))
                {
                    var apiResponse = await _httpClient.GetAsync("https://localhost:7109/api/ScheduleDetailApi/");
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        var responseContent = await apiResponse.Content.ReadAsStringAsync();
                        var schedules = JsonConvert.DeserializeObject<List<Scheduledetail>>(responseContent);

                        // Check UserRole nếu role = 1 là admin thì được quản lí 
                        var staff = await db.Staff.FirstOrDefaultAsync(s => s.StaffId == staffId);
                        if (staff != null)
                        {
                            if (staff.RoleId == 1)
                            {
                                // Display all schedules for UserRole 1
                                return View(schedules);
                            }
                            else
                            {
                                //lọc lịch làm việc dựa theo StaffId
                                var filteredSchedules = schedules.Where(s => s.Staff.StaffId == staffId).ToList();
                                return View(filteredSchedules);
                            }
                        }
                    }
                }
            }

            ViewBag.LoginMessage = "Please log in to view the schedule.";
            return View();
        }

    }
}
