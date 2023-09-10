using DoAn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
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

            ViewBag.LoginMessage = "Please login to view the schedule.";
            return View();
        }
        public IActionResult Create()
        {
            // Fetch the list of staff and schedules from your database
            var staffList = db.Staff.ToList();
            var scheduleList = db.Schedules.ToList();

            // Create SelectList items for the dropdowns
            ViewBag.StaffId = new SelectList(staffList, "StaffId", "Name"); // Change "StaffName" to the actual property you want to display for staff
            ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "Time"); // Change "ScheduleTime" to the actual property you want to display for schedules

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Scheduledetail inputModel)
        {
            try
            {
                if (inputModel.StaffId == 0 || inputModel.ScheduleId == 0)
                {
                    ModelState.AddModelError("", "Please select a Staff and a Schedule.");
                    var staffList = await db.Staff.ToListAsync();
                    var scheduleList = await db.Schedules.ToListAsync();
                    ViewBag.StaffId = new SelectList(staffList, "StaffId", "Name");
                    ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "Time"); 
                    return View(inputModel);
                }

                var selectedStaff = await db.Staff.FindAsync(inputModel.StaffId);
                var selectedSchedule = await db.Schedules.FindAsync(inputModel.ScheduleId);

                inputModel.Staff = selectedStaff;
                inputModel.Schedule = selectedSchedule;

                var serializedModel = JsonConvert.SerializeObject(inputModel);
                var content = new StringContent(serializedModel, Encoding.UTF8, "application/json");

                var apiResponse = await _httpClient.PostAsync("https://localhost:7109/api/ScheduleDetailApi/create", content);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var responseContent = await apiResponse.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<Scheduledetail>(responseContent);
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorResponse = await apiResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Failed to create Scheduledetail: " + errorResponse);
                    var staffList = await db.Staff.ToListAsync();
                    var scheduleList = await db.Schedules.ToListAsync();
                    ViewBag.StaffId = new SelectList(staffList, "StaffId", "StaffName");
                    ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "ScheduleTime");
                    return View(inputModel);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }

    }
}
