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
            var staffList = db.Staff.ToList();
            var scheduleList = db.Schedules.ToList();

            // Create select lists for dropdowns
            ViewBag.StaffId = new SelectList(staffList, "StaffId", "StaffId"); // Change "Name" to the actual property you want to display
            ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "ScheduleId"); // Change "Time" to the actual property you want to display

            return View();
        }

        // POST: /ScheduleDetail/Create
        [HttpPost]
        public async Task<IActionResult> Create(Scheduledetail viewModel)
        {
            if (!ModelState.IsValid)
            {
                var staffList = db.Staff.ToList();
                var scheduleList = db.Schedules.ToList();
                ViewBag.StaffId = new SelectList(staffList, "StaffId", "StaffId");
                ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "ScheduleId");

                return View(viewModel);
            }

            // Prepare data to send to the API
            var createModel = new Scheduledetail
            {
                StaffId = viewModel.StaffId,
                ScheduleId = viewModel.ScheduleId,
                Date = viewModel.Date,
                Status = viewModel.Status
            };

            // Serialize and send data to the API
            var serializedData = JsonConvert.SerializeObject(createModel);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");

            var apiResponse = await _httpClient.PostAsync("https://localhost:7109/api/ScheduleDetailApi/Create", content);

            if (apiResponse.IsSuccessStatusCode)
            {
                // Handle success
                return RedirectToAction("Index");
            }
            else
            {
                // Handle error
                ModelState.AddModelError(string.Empty, "An error occurred while creating the schedule detail.");
                return View(viewModel);
            }
        }

    }
}
