using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using System.Text;
using MailKit.Net.Smtp;

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

        [HttpGet]
        public IActionResult SendmailOutDated(int staffId)
        {
            ViewBag.StaffId = staffId;
            return View();
        }

        [HttpPost]
        public IActionResult SendmailOutDated(Mails model)
        {
            int staffId = int.Parse(Request.Form["staffId"]);
            var staffMember = db.Staff.FirstOrDefault(s => s.StaffId == staffId);
            var upcomingSchedule = db.Scheduledetails
                .Where(sd => sd.StaffId == staffId && sd.Date >= DateTime.Now)
                .OrderBy(sd => sd.Date)
                .Include(sd => sd.Schedule) 
                .FirstOrDefault();

            if (staffMember != null && upcomingSchedule != null && upcomingSchedule.Schedule != null)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Admin", "huynhhiepvan1998@gmail.com"));
                message.Subject = "Upcoming Work Schedule Notification";

                var staffName = upcomingSchedule.Staff.Name;
                var scheduleTime = upcomingSchedule.Schedule.Time;

                message.Body = new TextPart("html")
                {
                    Text = $"<html><body>" +
                           $"<p style=\"text-transform: uppercase;\"> <strong>You have Schedule To work</strong> </p>" +
                           $"<p><strong>staff Id: </strong> {upcomingSchedule.StaffId}</p>" +
                           $"<p><strong>Staff Name: </strong> {staffName}</p>" +
                           $"<p><strong>Time work: </strong> {scheduleTime}</p>" +
                           $"<p><strong>Date work:</strong> {upcomingSchedule.Date?.ToString("dd/MM/yyyy")}</p>" +
                           $"</body></html>"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(staffMember.Name, staffMember.Email));

                    client.Send(message);

                    client.Disconnect(true);
                }
            }

            return View();
        }

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

        //                var currentDate = DateTime.Now.Date;

        //                //loc ra ngay bes hon ngayf hiện tài thì sẽ xóatrong DB lun
        //                var outdatedSchedules = schedules.Where(s => s.Date < currentDate).ToList();
        //                foreach (var outdatedSchedule in outdatedSchedules)
        //                {
        //                    await Delete(outdatedSchedule.StaffId, outdatedSchedule.ScheduleId);
        //                }

        //                var staff = await db.Staff.FirstOrDefaultAsync(s => s.StaffId == staffId);
        //                if (staff != null)
        //                {
        //                    if (staff.RoleId == 1)
        //                    {
        //                        return View(schedules);
        //                    }
        //                    else
        //                    {
        //                        var filteredSchedules = schedules.Where(s => s.Staff.StaffId == staffId).ToList();
        //                        return View(filteredSchedules);
        //                    }
        //                }
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
                    ViewBag.StaffId = staffList.Select(s => new SelectListItem { Value = s.StaffId.ToString(), Text = s.Name });
                    ViewBag.ScheduleId = scheduleList.Select(s => new SelectListItem { Value = s.ScheduleId.ToString(), Text = s.Time.ToString() });
                    return View(inputModel);
                }

                if (!inputModel.Date.HasValue)
                {
                    ModelState.AddModelError("", "Please provide a valid Date.");
                    var staffList = await db.Staff.ToListAsync();
                    var scheduleList = await db.Schedules.ToListAsync();
                    ViewBag.StaffId = staffList.Select(s => new SelectListItem { Value = s.StaffId.ToString(), Text = s.Name });
                    ViewBag.ScheduleId = scheduleList.Select(s => new SelectListItem { Value = s.ScheduleId.ToString(), Text = s.Time.ToString() });
                    return View(inputModel);
                }


                var selectedStaff = await db.Staff.FindAsync(inputModel.StaffId);
                var selectedSchedule = await db.Schedules.FindAsync(inputModel.ScheduleId);

                inputModel.Staff = selectedStaff;
                inputModel.Schedule = selectedSchedule;

                var existingDetail = await db.Scheduledetails
                    .FirstOrDefaultAsync(sd =>
                        sd.StaffId == inputModel.StaffId &&
                        sd.ScheduleId == inputModel.ScheduleId);

                if (existingDetail != null)
                {
                    ModelState.AddModelError("", "A schedule detail already exists for the selected Staff and Schedule.");
                    var staffList = await db.Staff.ToListAsync();
                    var scheduleList = await db.Schedules.ToListAsync();
                    ViewBag.StaffId = staffList.Select(s => new SelectListItem { Value = s.StaffId.ToString(), Text = s.Name });
                    ViewBag.ScheduleId = scheduleList.Select(s => new SelectListItem { Value = s.ScheduleId.ToString(), Text = s.Time.ToString() });
                    return View(inputModel);
                }

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
                    ViewBag.StaffId = staffList.Select(s => new SelectListItem { Value = s.StaffId.ToString(), Text = s.Name });
                    ViewBag.ScheduleId = scheduleList.Select(s => new SelectListItem { Value = s.ScheduleId.ToString(), Text = s.Time.ToString() });
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

            ViewBag.StaffList = new SelectList(staffList, "StaffId", "Name");
            ViewBag.ScheduleList = new SelectList(scheduleList, "ScheduleId", "Time");

            var scheduleDetail = db.Scheduledetails
                .FirstOrDefault(sd => sd.ScheduleId == scheduleId && sd.StaffId == staffId);

            if (scheduleDetail == null)
            {
                return NotFound();
            }

            return View(scheduleDetail);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int scheduleId, int staffId, Scheduledetail updatedModel)
        {
            try
            {
                updatedModel.StaffId = staffId;
                updatedModel.ScheduleId = scheduleId;

                var staff = await db.Staff.FindAsync(staffId);
                var schedule = await db.Schedules.FindAsync(scheduleId);

                if (staff != null && schedule != null)
                {
                    updatedModel.Staff = staff;
                    updatedModel.Schedule = schedule;

                    var apiUrl = $"https://localhost:7109/api/ScheduleDetailApi/update?scheduleId={scheduleId}&staffId={staffId}";

                    var serializedModel = JsonConvert.SerializeObject(updatedModel);
                    var content = new StringContent(serializedModel, Encoding.UTF8, "application/json");

                    var apiResponse = await _httpClient.PutAsync(apiUrl, content);

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
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
                else
                {
                    return NotFound("Staff or Schedule not found.");
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


        public async Task<IActionResult> Delete(int staffId, int scheduleId)
        {
            try
            {
                var apiUrl = $"https://localhost:7109/api/ScheduleDetailApi/delete?staffId={staffId}&scheduleId={scheduleId}";

                var apiResponse = await _httpClient.DeleteAsync(apiUrl);

                if (apiResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorResponse = await apiResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Failed to delete Scheduledetail: " + errorResponse);
                    return RedirectToAction("Index");
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
