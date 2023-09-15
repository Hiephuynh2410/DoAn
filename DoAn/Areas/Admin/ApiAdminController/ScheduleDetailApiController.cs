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
                    Status = true,
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
                    },
                    Schedule = new
                    {
                        Time = createModel.Schedule?.Time,
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

        [HttpPut("update")]
        public async Task<IActionResult> UpdateScheduleDetail([FromQuery] int scheduleId, [FromQuery] int staffId, Scheduledetail updatedModel)
        {
            try
            {
                var existingScheduleDetail = await db.Scheduledetails
                    .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.StaffId == staffId);

                if (existingScheduleDetail == null)
                {
                    return NotFound(new
                    {
                        Message = "Schedule Detail not found."
                    });
                }

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

                existingScheduleDetail.Date = updatedModel.Date;
                existingScheduleDetail.Status = updatedModel.Status;

                await db.SaveChangesAsync();

                var scheduleDetailIds = await db.Scheduledetails
                    .Select(sd => new { sd.ScheduleId, sd.StaffId })
                    .ToListAsync();

                return Ok(scheduleDetailIds);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An unexpected error occurred. Please try again later."
                });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteScheduleDetail([FromQuery] int staffId, [FromQuery] int scheduleId)
        {
            try
            {
                var matchingScheduledetails = await db.Scheduledetails
                    .Where(s => s.StaffId == staffId && s.ScheduleId == scheduleId)
                    .ToListAsync();

                if (matchingScheduledetails == null || !matchingScheduledetails.Any())
                {
                    return NotFound(new
                    {
                        Message = "Scheduledetail not found."
                    });
                }

                db.Scheduledetails.RemoveRange(matchingScheduledetails);
                await db.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Scheduledetail deleted successfully."
                });
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
