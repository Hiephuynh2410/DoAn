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

                        var staff = await db.Staff.FirstOrDefaultAsync(s => s.StaffId == staffId);
                        if (staff != null)
                        {
                            if (staff.RoleId == 1)
                            {
                                return View(schedules);
                            }
                            else
                            {
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

        public IActionResult Create()
        {
            var staffList = db.Staff.ToList();
            var scheduleList = db.Schedules.ToList();

            ViewBag.StaffId = new SelectList(staffList, "StaffId", "Name");
            ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "Time"); 

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

        [HttpGet]
        public IActionResult edit(int scheduleId, int staffId)
        {
            var staffList = db.Staff.ToList();
            var scheduleList = db.Schedules.ToList();
            ViewBag.StaffId = new SelectList(staffList, "StaffId", "Name");
            ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "Time");
            var scheduleDetail = db.Scheduledetails.Find(scheduleId, staffId);
            if (scheduleDetail == null)
            {
                return NotFound();
            }
            return View(scheduleDetail);
        }
        [HttpPost]
        public async Task<IActionResult> edit(int scheduleId, int staffId, Scheduledetail updatedModel)
        {
            try
            {
                updatedModel.StaffId = staffId;
                updatedModel.ScheduleId = scheduleId;

                var apiUrl = $"https://localhost:7109/api/ScheduleDetailApi/update?scheduleId={scheduleId}&staffId={staffId}";

                var serializedModel = JsonConvert.SerializeObject(updatedModel);
                var content = new StringContent(serializedModel, Encoding.UTF8, "application/json");

                var apiResponse = await _httpClient.PutAsync(apiUrl, content);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var updatedScheduleDetail = await db.Scheduledetails.FindAsync(scheduleId, staffId);

                    if (updatedScheduleDetail != null)
                    {
                        var staffList = db.Staff.ToList();
                        var scheduleList = db.Schedules.ToList();
                        ViewBag.StaffId = new SelectList(staffList, "StaffId", "Name");
                        ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "Time");

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return NotFound(); 
                    }
                }
                else
                {
                    var errorResponse = await apiResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Failed to update Scheduledetail: " + errorResponse);

                    var staffList = db.Staff.ToList();
                    var scheduleList = db.Schedules.ToList();
                    ViewBag.StaffId = new SelectList(staffList, "StaffId", "Name");
                    ViewBag.ScheduleId = new SelectList(scheduleList, "ScheduleId", "Time");

                    return View(updatedModel);
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
