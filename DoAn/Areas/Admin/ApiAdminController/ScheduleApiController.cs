using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public ScheduleApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSchedule()
        {
            var Schedules = await _dbContext.Schedules
                .Include(s => s.Scheduledetails)
                .ToListAsync();

            var SchedulesWithFullInfo = Schedules.Select(s => new
            {
                s.ScheduleId,
                s.Time,
            }).ToList();
            return Ok(SchedulesWithFullInfo);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule([FromBody] Schedule createModel)
        {
            if (ModelState.IsValid)
            {
                var ScheduleExists = await _dbContext.Schedules.AnyAsync(b => b.Time == createModel.Time);
                if (ScheduleExists)
                {
                    return BadRequest(new { Message = "Schedules already exists." });
                }

                var newSchedule = new Schedule
                {
                    ScheduleId = createModel.ScheduleId,
                    Time = createModel.Time,
                };

                _dbContext.Schedules.Add(newSchedule);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Schedules registration successful",
                    ScheduleId = newSchedule.ScheduleId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid Schedule data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }
        [HttpPut("update/{ScheduleId}")]
        public async Task<IActionResult> UpdateSchedul(int ScheduleId, Schedule updateModel)
        {
            var Schedule = await _dbContext.Schedules.FindAsync(ScheduleId);
            if (Schedule == null)
            {
                return NotFound();
            }

            if (updateModel.Time.HasValue)
            {
                Schedule.Time = updateModel.Time;
            }
            _dbContext.Entry(Schedule).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Schedule updated successfully"
            };

            return Ok(updateSuccessResponse);
        }
        [HttpDelete("delete/{ScheduleId}")]
        public async Task<IActionResult> DeleteSchedule(int ScheduleId)
        {
            var Schedules = await _dbContext.Schedules.FindAsync(ScheduleId);
            if (Schedules == null)
            {
                return NotFound();
            }

            _dbContext.Schedules.Remove(Schedules);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Schedule deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }
        [HttpGet("detail/{ScheduleId}")]
        public async Task<IActionResult> GetScheduleDetail(int ScheduleId)
        {
            var schedule = await _dbContext.Schedules.FindAsync(ScheduleId);
            if (schedule == null)
            {
                return NotFound();
            }
            var scheduleDetail = new
            {
                ScheduleId = schedule.ScheduleId,
                Time = schedule.Time.ToString()
            };

            return Ok(scheduleDetail);
        }
    }
}
