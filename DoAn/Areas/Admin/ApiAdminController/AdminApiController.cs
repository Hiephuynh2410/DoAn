using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : Controller
    {
        private readonly DlctContext _dbContext;
        private readonly IConfiguration _configuration;

        public AdminApiController(DlctContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var user = await _dbContext.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(u => u.Username == loginModel.Username);

            if (user == null || !VerifyPasswordHash(loginModel.Password, user.Password))
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);

            var response = new
            {
                UserId = user.StaffId,
                RoleId = user.Role?.Name,
                Token = token
            };

            return Ok(response);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var passwordHasher = new PasswordHasher<Staff>();
            var result = passwordHasher.VerifyHashedPassword(null, storedHash, password);
            return result == PasswordVerificationResult.Success;
        }

        private string GenerateJwtToken(Staff user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.StaffId.ToString()),
                new Claim(ClaimTypes.Role, user.Role?.RoleId.ToString()), // Assuming RoleId is an integer
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffs = await _dbContext.Staff
                .Include(s => s.Branch)
                .Include(s => s.Role)
                .Include(s => s.Scheduledetails)
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
                s.IsDisabled,
                s.CreatedAt,
                s.RoleId,
                s.BranchId,
                Branch = new
                {
                    s.Branch?.Address,
                    s.Branch?.Hotline
                },
                Role = new
                {
                    s.Role?.Name,
                    s.Role?.RoleId
                },

                s.UpdatedAt,
                s.CreatedBy,
                s.UpdatedBy,
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
                    IsDisabled = false,
                    CreatedAt = DateTime.Now,
                    CreatedBy = registrationModel.CreatedBy,
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

            staff.IsDisabled = true;
            staff.Status = false;
            _dbContext.Entry(staff).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Staff disabled successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpPut("reload/{staffId}")]
        public async Task<IActionResult> ReloadStaff(int staffId)
        {
            var staff = await _dbContext.Staff
                .FirstOrDefaultAsync(p => p.StaffId == staffId);

            if (staff == null)
            {
                return NotFound();
            }

            staff.IsDisabled = false;
            staff.Status = true;
            _dbContext.Entry(staff).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var reloadSuccessResponse = new
            {
                Message = "Staff reloaded successfully"
            };

            return Ok(reloadSuccessResponse);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStaff(string keyword)
        {
            var staffs = await _dbContext.Staff
                .Include(s => s.Branch)
                .Include(s => s.Role)
                .Where(s =>
                    s.Name.Contains(keyword) || s.StaffId.ToString() == keyword
                )
                .ToListAsync();

            var staffsWithFullInfo = staffs.Select(s => new
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

            return Ok(staffsWithFullInfo);
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
                staff.IsDisabled,
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
