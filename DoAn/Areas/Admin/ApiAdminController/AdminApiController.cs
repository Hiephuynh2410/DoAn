using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
            var staffs = _dbContext.Staff.ToList();
            return Ok(staffs);
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(Staff loginModel)
        {
            var staff = await _dbContext.Staff.FirstOrDefaultAsync(c => c.Username == loginModel.Username);
            if (staff == null)
            {
                var loginErrorResponse = new
                {
                    Message = "Invalid username or password",
                    Errors = new List<string>
                    {
                        "Invalid username"
                    }
                };

                return BadRequest(loginErrorResponse);
            }
            if (string.IsNullOrWhiteSpace(loginModel.Username) && string.IsNullOrWhiteSpace(loginModel.Password))
            {
                var nameErrorRespone = new
                {
                    Message = "Name and password cannot be empty"
                };
                return BadRequest(nameErrorRespone);
            }

            var passwordHasher = new PasswordHasher<Staff>();
            var result = passwordHasher.VerifyHashedPassword(null, staff.Password, loginModel.Password);
            if (result == PasswordVerificationResult.Success)
            {
                var loginSuccessResponse = new
                {
                    Message = "Login successful"
                };

                return Ok(loginSuccessResponse);
            }

            var invalidLoginErrorResponse = new
            {
                Message = "Invalid username or password",
                Errors = new List<string>
                {
                    "Invalid password"
                }
            };

            return BadRequest(invalidLoginErrorResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterStaff(Staff registrationModel)
        {
            if (ModelState.IsValid)
            {
                //kiểm tra email có trùng lặp trong DB không
                if (_dbContext.Staff.Any(c => c.Email == registrationModel.Email))
                {
                    var emailExistsResponse = new
                    {
                        Message = "Email already exists"
                    };


                    return Conflict(emailExistsResponse);
                }
                if (string.IsNullOrWhiteSpace(registrationModel.Name)
                    || string.IsNullOrWhiteSpace(registrationModel.Username)
                    || string.IsNullOrWhiteSpace(registrationModel.Password)
                    || string.IsNullOrWhiteSpace(registrationModel.Phone)
                    || string.IsNullOrWhiteSpace(registrationModel.Email))
                {
                    var nameErrorRespone = new
                    {
                        Message = "Name, username, password, phone, and email cannot be empty"
                    };
                    return BadRequest(nameErrorRespone);
                }
                if (registrationModel.Name.Length > 100)
                {
                    var nameErrorRespone = new
                    {
                        Message = "Name is too long"
                    };
                    return BadRequest(nameErrorRespone);
                }
                if (!Regex.IsMatch(registrationModel.Phone, @"^(?:\+84|0)\d{9,10}$"))
                {
                    var phoneErrorRespone = new
                    {
                        Message = "Invalid phone number format"
                    };
                    return BadRequest(phoneErrorRespone);
                }
                if (!Regex.IsMatch(registrationModel.Username, "^[]A-Za-z0-9!#$%&'*+/=?^_`{|}~\\,.@()<>[-]*$"))
                {
                    var usernameErroeRespone = new
                    {
                        Message = "Invalid username format,username needs 8 digits and has 1 special character and 1 uppercase character"
                    };
                    return BadRequest(usernameErroeRespone);
                }
                var passwordHasher = new PasswordHasher<Staff>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                var newStaff = new Staff
                {
                    Name = registrationModel.Name,
                    Username = registrationModel.Username,
                    Password = hashedPassword,
                    Phone = registrationModel.Phone,
                    Address = registrationModel.Address,
                    Avatar = registrationModel.Avatar,
                    Email = registrationModel.Email,
                };

                _dbContext.Staff.Add(newStaff);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    ClientId = newStaff.StaffId
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
