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
        private readonly DlctContext _dbContext;
        public ScheduleDetailApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllScheduleDetails()
        {
            var Schedules = await _dbContext.Scheduledetails
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
        public async Task<IActionResult> CreateScheduleDetail(Scheduledetail createModel)
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

                if (!createModel.Date.HasValue)
                {
                    return BadRequest(new { Message = "Date is required." });
                }

                if (!IsValidDateFormat(createModel.Date.Value, "MM/dd/yyyy"))
                {
                    return BadRequest(new { Message = "Invalid date format. Use MM/dd/yyyy." });
                }

                if (await ScheduleDetailExists(createModel.Date.Value))
                {
                    return BadRequest(new { Message = "Schedule Detail already exists for the given date." });
                }

                var newScheduleDetail = new Scheduledetail
                {
                    StaffId = createModel.StaffId,
                    ScheduleId = createModel.ScheduleId,
                    Date = createModel.Date,
                    Status = createModel.Status,
                };

                _dbContext.Scheduledetails.Add(newScheduleDetail);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Schedule Detail registration successful",
                    ScheduleDetailId = newScheduleDetail.ScheduleId
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

        private bool IsValidDateFormat(DateTime date, string format)
        {
            return DateTime.TryParseExact(date.ToString(format), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        private async Task<bool> ScheduleDetailExists(DateTime date)
        {
            return await _dbContext.Scheduledetails.AnyAsync(b => b.Date == date);
        }

    }
}
