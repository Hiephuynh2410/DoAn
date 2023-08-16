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
        public async Task<IActionResult> RegisterStaff(Staff  registrationModel)
        {
            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Staff>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);
                var Role = await _dbContext.Roles.FindAsync(registrationModel.RoleId);

                var newStaff = new Staff
                {
                    Name = registrationModel.Name,
                    Username = registrationModel.Username,
                    Password = hashedPassword,
                    Phone = registrationModel.Phone,
                    Address = registrationModel.Address,
                    Avatar = registrationModel.Avatar,
                    Email = registrationModel.Email,
                    Branch = branch,
                    Role = Role,
                };

                _dbContext.Staff.Add(newStaff);
                await _dbContext.SaveChangesAsync();
                _dbContext.Entry(newStaff).Reference(s => s.Branch).Load();
                _dbContext.Entry(newStaff).Reference(s => s.Role).Load();
                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    ClientId = newStaff.StaffId,
                    //Name = newStaff.Name,
                    //Username = newStaff.Username,
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
            var Staff = await _dbContext.Staff.FindAsync(staffId);

            if (Staff == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                Staff.Name = updateModel.Name;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Username))
            {
                Staff.Username = updateModel.Username;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Phone))
            {
                Staff.Phone = updateModel.Phone;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Address))
            {
                Staff.Address = updateModel.Address;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Avatar))
            {
                Staff.Avatar = updateModel.Avatar;
            }
            if (!string.IsNullOrWhiteSpace(updateModel.Email))
            {
                //kiểm tra email có trùng lặp trong DB không
                if (_dbContext.Staff.Any(c => c.Email == updateModel.Email && c.StaffId != staffId))
                {
                    var emailExistsResponse = new
                    {
                        Message = "Email already exists"
                    };

                    return Conflict(emailExistsResponse);
                }

                Staff.Email = updateModel.Email;
            }

            _dbContext.Entry(Staff).State = EntityState.Modified;
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
            var staff = await _dbContext.Staff.FindAsync(staffId);

            if (staff == null)
            {
                return NotFound();
            }
            var stafftDetail = new
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
                staff.UpdatedBy
            };
            return Json(stafftDetail);
        }

    }
}
