using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace DoAn.Areas.Admin.Services
{
    public class StaffServives
    {
        private readonly DlctContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StaffServives(DlctContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<IActionResult> UpdateStaffAsync(int staffId, Staff updateModel)
        {
            var staff = await _dbContext.Staff
                .FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (staff == null)
            {
                return new NotFoundResult();
            }

            staff.Name = updateModel.Name;
            staff.Username = updateModel.Username;
            staff.Phone = updateModel.Phone;
            staff.Address = updateModel.Address;
            staff.Avatar = updateModel.Avatar;
            staff.Status = updateModel.Status;
            staff.UpdatedAt = DateTime.Now;
            staff.UpdatedBy = updateModel.UpdatedBy;
            if (updateModel.BranchId != staff.BranchId)
            {
                var newBranch = await _dbContext.Branches.FindAsync(updateModel.BranchId);
                if (newBranch != null)
                {
                    staff.Branch = newBranch;
                }
            }

            if (updateModel.RoleId != staff.RoleId)
            {
                var newRole = await _dbContext.Roles.FindAsync(updateModel.RoleId);
                if (newRole != null)
                {
                    staff.Role = newRole;
                }
            }

            _dbContext.Entry(staff).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Staff updated successfully"
            };

            return new OkObjectResult(updateSuccessResponse);
        }

        public async Task<List<object>> searchStaff(string keyword)
        {
            var staffs = await _dbContext.Staff
                .Include(s => s.Branch)
                .Include(s => s.Role)
                .Where(s =>
                    s.Name.Contains(keyword) || s.StaffId.ToString() == keyword
                )
                .ToListAsync();

            var staffsWithFullInfo = staffs.Select(s => (object)new
            {
                s.StaffId,
                s.Name,
                s.Username,
                s.Phone,
                s.Address,
                s.Avatar,
                s.Email,
                s.Status,
                s.IsDisabled,
                s.CreatedAt,
                s.UpdatedAt,
                s.CreatedBy,
                s.UpdatedBy,
                Branch = new
                {
                    Address = s.Branch?.Address,
                    Hotline = s.Branch?.Hotline
                },
                Role = new
                {
                    Name = s.Role?.Name,
                    RoleId = s.Role?.RoleId
                }
            }).ToList();

            return staffsWithFullInfo;
        }

        public async Task<List<object>> GetAllStaff()
        {
            var staff = await _dbContext.Staff
                .Include(x => x.Branch)
                .Include(x => x.Role)
                .ToListAsync();
            return staff.Select(x => new
            {
                x.StaffId,
                x.Name,
                x.Username,
                x.Password,
                x.Phone,
                x.Avatar,
                x.Address,
                x.Email,
                x.Status,
                x.IsDisabled,
                x.RoleId,
                x.CreatedAt,
                x.UpdatedAt,
                x.CreatedBy,
                x.UpdatedBy,
                Branch = new
                {
                    BranchId = x.Branch?.BranchId,
                    Address = x.Branch?.Address,
                    Hotline = x.Branch?.Hotline,
                },
                Role = new
                {
                    RoleId = x.Role?.RoleId,
                    Name = x.Role?.Name,    
                }
            }).Cast<object>().ToList();
        }

        [HttpDelete("delete/{staffId}")]
        public async Task<IActionResult> DeleteStaff(int staffId)
        {
            var staff = await _dbContext.Staff.FindAsync(staffId);

            if (staff == null)
            {
                return new NotFoundResult();
            }   

            staff.IsDisabled = true;
            staff.Status = false;
            _dbContext.Entry(staff).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Staff disabled successfully"
            };

            return new OkObjectResult(deleteSuccessResponse);
        }

        [HttpPut("add/{staffId}")]
        public async Task<IActionResult> AddStaff(int staffId)
        {
            var staff = await _dbContext.Staff
                .FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (staff == null)
            {
                return new NotFoundResult();
            }

            staff.IsDisabled = false;
            staff.Status = true;
            _dbContext.Entry(staff).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var reloadSuccessResponse = new
            {
                Message = "Staff add successfully"
            };

            return new OkObjectResult(reloadSuccessResponse);
        }
    }
}
