using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace DoAn.Areas.Admin.Services
{
    public class StaffServives
    {
        private readonly DlctContext _dbContext;

        public StaffServives(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IActionResult> UpdateStaffAsync(int staffId, Staff updateModel)
        {
            var staffToUpdate = await _dbContext.Staff
                .Include(p => p.Branch)
                .Include(p => p.Role)
                .FirstOrDefaultAsync(p => p.StaffId == staffId);
            if (staffToUpdate == null)
            {
                return new NotFoundObjectResult("Not found staff");
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                staffToUpdate.Name = updateModel.Name;
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Username))
            {
                staffToUpdate.Username = updateModel.Username;
            }

            if (updateModel.BranchId.HasValue)
            {
                var updatedProductType = await _dbContext.Branches.FindAsync(updateModel.BranchId);
                if (updatedProductType != null)
                {
                    staffToUpdate.Branch = updatedProductType;
                }
            }

            if (updateModel.RoleId.HasValue)
            {
                var updatedProvider = await _dbContext.Roles.FindAsync(updateModel.RoleId);
                if (updatedProvider != null)
                {
                    staffToUpdate.Role = updatedProvider;
                }
            }

            staffToUpdate.UpdatedAt = DateTime.Now;
            staffToUpdate.UpdatedBy = updateModel.UpdatedBy;

            _dbContext.Entry(staffToUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Product updated successfully"
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
    }
}
