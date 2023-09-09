using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleDetailApiController : Controller
    {
        private readonly DlctContext db;
        public ScheduleDetailApiController(DlctContext dbContext)
        {
            db = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllScheduleDetails()
        {
            var Schedules = await db.Scheduledetails
                 .Include(s => s.Staff)
                 .Include(s => s.Schedule)
                .ToListAsync();

            var SchedulesWithFullInfo = Schedules.Select(s => new
            {
                s.ScheduleId,
                s.Date,
                s.StaffId,
                Staff = new
                {
                    s.Staff.StaffId,
                    s.Staff.Name
                },
                Schedule = s.Schedule != null ? new
                {
                    s.Schedule.ScheduleId,
                    s.Schedule.Time
                } : null
            }).ToList();
            return Ok(SchedulesWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateScheduleDetail(Scheduledetail inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid Schedule Detail data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Set the staff and schedule properties based on the inputModel
                var staff = await db.Staff.FindAsync(inputModel.StaffId);
                var schedule = await db.Schedules.FindAsync(inputModel.ScheduleId);

                var createModel = new Scheduledetail
                {
                    StaffId = inputModel.StaffId,
                    ScheduleId = inputModel.ScheduleId,
                    Date = inputModel.Date,
                    Status = inputModel.Status,
                    Staff = staff,
                    Schedule = schedule
                };

                db.Scheduledetails.Add(createModel);
                await db.SaveChangesAsync();

                // Return the response as before
                var registrationSuccessResponse = new
                {
                    Message = "Schedule Detail registration successful",
                    ScheduleDetailId = createModel.ScheduleId,
                    Staff = new
                    {
                        StaffId = createModel.Staff?.StaffId,
                        // Include other staff properties as needed
                    },
                    Schedule = new
                    {
                        Time = createModel.Schedule?.Time,
                        // Include other schedule properties as needed
                    }
                };

                return Ok(registrationSuccessResponse);
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
