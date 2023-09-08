using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                 .Include(s => s.Schehule)
                .ToListAsync();

            var SchedulesWithFullInfo = Schedules.Select(s => new
            {
                s.SchehuleId,
                s.Date,
                s.StaffId,
                Staff = new
                {
                    s.Staff.StaffId,
                    s.Staff.Name
                },
                Schedule = s.Schehule != null ? new
                {
                    s.Schehule.ScheduleId,
                    s.Schehule.Time
                } : null
            }).ToList();
            return Ok(SchedulesWithFullInfo);
        }
    }
}
