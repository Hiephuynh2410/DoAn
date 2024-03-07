using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.ApiController.Services
{
    public class BookingDateServices
    {
        private readonly DlctContext _dbContext;

        public BookingDateServices(DlctContext dbContext) 
        { 
            _dbContext = dbContext;
        }

        public async Task<IActionResult> GetStaffAndSchedule(int staffId)
        {
            try
            {
                // Lấy thông tin nhân viên
                var staff = await _dbContext.Staff
                    .Where(s => s.StaffId == staffId)
                    .Select(s => new
                    {
                        s.StaffId,
                        s.Name,
                        s.Phone,
                        s.Email,
                        s.Address,
                        s.Avatar,
                        s.Status,
                        s.RoleId,
                        s.BranchId
                    })
                    .FirstOrDefaultAsync();

                if (staff == null)
                {
                    return new NotFoundObjectResult($"Không tìm thấy thông tin cho nhân viên có ID: {staffId}");
                }

                // Lấy lịch làm việc của nhân viên
                var schedule = await _dbContext.Scheduledetails
                    .Include(sd => sd.Schedule)
                    .Where(sd => sd.StaffId == staffId)
                    .Select(sd => new
                    {
                        sd.Schedule.ScheduleId,
                        sd.Schedule.Time,
                        sd.Date,
                        sd.Status
                    })
                    .ToListAsync();

                var result = new
                {
                    Staff = staff,
                    Schedule = schedule
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Đã xảy ra lỗi: {ex.Message}") { StatusCode = 500 };
            }
        }
    }
}
