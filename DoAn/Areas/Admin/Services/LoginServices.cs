using DoAn.Models;
using DoAn.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Services
{
    public class LoginServices
    {
        private readonly DlctContext _dlctContext;

        private readonly GenerateRandomKey _generateRandomKey;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginServices(DlctContext dlctContext, GenerateRandomKey ganerateRandomKey, IHttpContextAccessor httpContextAccessor)
        {
            _dlctContext = dlctContext;
            _generateRandomKey = ganerateRandomKey;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> RegisterClient(Staff registrationModel)
        {
            if (registrationModel == null)
            {
                return new BadRequestObjectResult("Registration data is empty.");
            }

            if (string.IsNullOrWhiteSpace(registrationModel.Username) ||
                string.IsNullOrWhiteSpace(registrationModel.Password) ||
                string.IsNullOrWhiteSpace(registrationModel.Name) ||
                string.IsNullOrWhiteSpace(registrationModel.Phone) ||
                string.IsNullOrWhiteSpace(registrationModel.Email))
            {
                var emptyFieldsErrorResponse = new
                {
                    Message = "Không được để trống !",
                };
                return new BadRequestObjectResult(emptyFieldsErrorResponse);
            }
            if (registrationModel.RoleId == null || registrationModel.BranchId == null)
            {
                var missingFieldsErrorResponse = new
                {
                    Message = "RoleId và BranchId không được để trống !",
                };
                return new BadRequestObjectResult(missingFieldsErrorResponse);
            }
            var createdStaff = await _dlctContext.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(p => p.StaffId == registrationModel.StaffId || p.Username == registrationModel.Username);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registrationModel.Password);

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
                Role = createdStaff?.Role,
                Branch = createdStaff?.Branch
            }; 
            if (registrationModel.RoleId != null)
            {
                var role = await _dlctContext.Roles.FindAsync(registrationModel.RoleId);
                if (role == null)
                {
                    return new BadRequestObjectResult("Invalid RoleId provided.");
                }
                newStaff.Role = role;
            }

            if (registrationModel.BranchId != null)
            {
                var branch = await _dlctContext.Branches.FindAsync(registrationModel.BranchId);
                if (branch == null)
                {
                    return new BadRequestObjectResult("Invalid BranchId provided.");
                }
                newStaff.Branch = branch;
            }

            _dlctContext.Staff.Add(newStaff);
            await _dlctContext.SaveChangesAsync();

            _dlctContext.Entry(newStaff).Reference(c => c.Branch).Load();
            _dlctContext.Entry(newStaff).Reference(c => c.Role).Load();

            var registrationSuccessResponse = new
            {
                Message = "Registration successful",
                ClientId = newStaff.StaffId,
                Role = new
                {
                    Name = newStaff.Role?.Name,
                    RoleId = newStaff.Role?.RoleId
                }
            };
            return new OkObjectResult(registrationSuccessResponse);
        }

        public async Task<IActionResult> Login(Staff loginModel)
        {
            if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                var errorResponse = new
                {
                    Message = "Username and password cannot be empty"
                };
                return new BadRequestObjectResult(errorResponse);
            }

            var staff = await _dlctContext.Staff.FirstOrDefaultAsync(c => c.Username == loginModel.Username);

            if (staff == null)
            {
                var loginErrorResponse = new
                {
                    Message = "Invalid username or password",
                };
                return new BadRequestObjectResult(loginErrorResponse);
            }

            if (staff.FailedLoginAttemps >= 5 && staff.LastFailedLoginAttempts != null)
            {
                var timeSinceLastFailedAttempt = DateTime.Now - staff.LastFailedLoginAttempts.Value;
                if (timeSinceLastFailedAttempt.TotalMinutes <= 5)
                {
                    var lockAccountResponse = new
                    {
                        Message = "Account is locked for 5 minutes. Please try again later."
                    };
                    return new BadRequestObjectResult(lockAccountResponse);
                }
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginModel.Password, staff.Password);

            if (isPasswordValid)
            {
                staff.FailedLoginAttemps = 0;
                staff.LastFailedLoginAttempts = null;
                _dlctContext.SaveChanges();

                string token = _generateRandomKey.CreateToken(staff);

                // Lấy HttpContext từ IHttpContextAccessor
                var httpContext = _httpContextAccessor.HttpContext;

                // Lưu thông tin vào session
                httpContext.Session.SetString("Username", staff.Username);
                if (staff.Avatar != null)
                {
                    httpContext.Session.SetString("Avatar", staff.Avatar);
                }
                httpContext.Session.SetString("UserId", staff.StaffId.ToString());
                httpContext.Session.SetString("Role", staff.RoleId.ToString());
                httpContext.Session.SetString("Name", staff.Name);

                var loginSuccessResponse = new
                {
                    Token = token,
                    Message = "Login successful"
                };

                return new OkObjectResult(loginSuccessResponse);
            }
            else
            {
                staff.FailedLoginAttemps++;
                staff.LastFailedLoginAttempts = DateTime.Now;
                _dlctContext.SaveChanges();

                var invalidLoginErrorResponse = new
                {
                    Message = "Invalid username or password",
                    Errors = new List<string>
            {
                "Invalid password"
            }
                };

                return new BadRequestObjectResult(invalidLoginErrorResponse);
            }
        }

        public async Task<IActionResult> GetStaffInfoById(int staffId)
        {
            if (staffId <= 0)
            {
                return new BadRequestObjectResult("Invalid staff ID.");
            }

            var staff = await _dlctContext.Staff.FindAsync(staffId);

            if (staff == null)
            {
                return new NotFoundObjectResult("Staff not found.");
            }

            var staffInfo = new
            {
                StaffId = staff.StaffId,
                Username = staff.Username,
                Name = staff.Name,
                Phone = staff.Phone,
                Email = staff.Email,
                avatar = staff.Avatar,

                // Add other properties as needed
            };

            return new OkObjectResult(staffInfo);
        }

        
    }
}
