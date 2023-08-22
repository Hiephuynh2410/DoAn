using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public AdminApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffs = await _dbContext.Staff
                .Include(s => s.Branch)
                .Include(s => s.Role)
                .ToListAsync();

            var staffsWithFullInfo = staffs.Select(s => new
            {
                s.StaffId,
                s.Name,
                s.Username,
                s.Password,
                s.Phone,
                s.Avatar,
                s.Address,
                s.Email,
                s.Status,
                s.RoleId,
                s.BranchId,
                Branch = new
                {
                    s.Branch.Address,
                    s.Branch.Hotline
                },
                Role = new
                {
                    s.Role.Name,
                    s.Role.RoleId
                },
                s.CreatedAt,
                s.UpdatedAt,
                s.CreatedBy,
                s.UpdatedBy,
                //s.Bookings,
            }).ToList();

            return Ok(staffsWithFullInfo);
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterStaff(Staff registrationModel)
        {
            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Staff>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);
                var role = await _dbContext.Roles.FindAsync(registrationModel.RoleId);

                var newStaff = new Staff
                {
                    Name = registrationModel.Name,
                    Username = registrationModel.Username,
                    Password = hashedPassword,
                    Phone = registrationModel.Phone,
                    Address = registrationModel.Address,
                    Avatar = registrationModel.Avatar,
                    Email = registrationModel.Email,
                    Status = registrationModel.Status,
                    Branch = branch,
                    Role = role,
                };

                _dbContext.Staff.Add(newStaff);
                await _dbContext.SaveChangesAsync();

                _dbContext.Entry(newStaff).Reference(s => s.Branch).Load();
                _dbContext.Entry(newStaff).Reference(s => s.Role).Load();

                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    ClientId = newStaff.StaffId,
                    Branch = new
                    {
                        Address = newStaff.Branch?.Address,
                        Hotline = newStaff.Branch?.Hotline
                    },
                    Role = new
                    {
                        Name = newStaff.Role?.Name,
                        RoleId = newStaff.Role?.RoleId
                    }
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid registration data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }



        [HttpPut("update/{staffId}")]
        public async Task<IActionResult> UpdateStaff(int staffId, Staff updateModel)
        {
            var staff = await _dbContext.Staff
                .FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (staff == null)
            {
                return NotFound();
            }

            // Update other properties...
            staff.Name = updateModel.Name;
            staff.Username = updateModel.Username;
            staff.Phone = updateModel.Phone;
            staff.Address = updateModel.Address;
            staff.Avatar = updateModel.Avatar;
            staff.Status = updateModel.Status;

            // Update related branch and role properties
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

            return Ok(updateSuccessResponse);
        }


        [HttpDelete("delete/{staffId}")]
        public async Task<IActionResult> DeleteStaff(int staffId)
        {
            var staff = await _dbContext.Staff.FindAsync(staffId);

            if (staff == null)
            {
                return NotFound();
            }

            _dbContext.Staff.Remove(staff);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "staff deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet("detail/{staffId}")]
        public async Task<IActionResult> GetClientDetail(int staffId)
        {
            var staff = await _dbContext.Staff
                .Include(s => s.Branch)
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.StaffId == staffId);

            if (staff == null)
            {
                return NotFound();
            }

            var staffDetail = new
            {
                staff.StaffId,
                staff.Name,
                staff.Username,
                staff.Phone,
                staff.Address,
                staff.Avatar,
                staff.Email,
                staff.Status,
                staff.CreatedAt,
                staff.UpdatedAt,
                staff.CreatedBy,
                staff.UpdatedBy,
                Branch = new
                {
                    staff.Branch?.Address,
                    staff.Branch?.Hotline
                },
                Role = new
                {
                    staff.Role?.Name,
                    staff.Role?.RoleId
                }
            };

            return Json(staffDetail);
        }

    }
}
