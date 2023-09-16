using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientLoginController : Controller
    {
        private readonly DlctContext _dbContext;
        public ClientLoginController(DlctContext dbContext)
        {
            _dbContext = dbContext;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Client loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                var errorResponse = new
                {
                    Message = "Username and password cannot be empty"
                };
                return BadRequest(errorResponse);
            }

            var client = await _dbContext.Clients.FirstOrDefaultAsync(c => c.Username == loginModel.Username);
            if (client == null)
            {
                var loginErrorResponse = new
                {
                    Message = "wrong pass or usenmae",
                    Errors = new List<string>
            {
                "Invalid username"
            }
                };
                return BadRequest(loginErrorResponse);
            }

            var passwordHasher = new PasswordHasher<Client>();
            var result = passwordHasher.VerifyHashedPassword(null, client.Password, loginModel.Password);

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
                Message = "wrong pass or usenmae",
                Errors = new List<string>
                {
                    "Invalid password"
                }
            };

            return BadRequest(invalidLoginErrorResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterClient(Client registrationModel)
        {
            if (registrationModel == null)
            {
                return BadRequest("Registration data is empty.");
            }

            if (string.IsNullOrWhiteSpace(registrationModel.Username) ||
                string.IsNullOrWhiteSpace(registrationModel.Password) ||
                string.IsNullOrWhiteSpace(registrationModel.Name) ||
                string.IsNullOrWhiteSpace(registrationModel.Phone) ||
                string.IsNullOrWhiteSpace(registrationModel.Email))
            {
                var emptyFieldsErrorResponse = new
                {
                    Message = "Empty fields in registration data.",
                    Errors = new List<string>
            {
                "Username, Password, Name, Phone, and Email are required fields."
            }
                };
                return BadRequest(emptyFieldsErrorResponse);
            }
            var vietnamesePhoneNumberPattern = @"^(0[0-9]{9,10})$"; 
            if (!Regex.IsMatch(registrationModel.Phone, vietnamesePhoneNumberPattern))
            {
                var phoneFormatErrorResponse = new
                {
                    Message = "Invalid phone number format",
                    Errors = new List<string>
            {
                "Phone number must be in the format 0XXXXXXXXX or 0XXXXXXXXXX (10 or 11 digits)."
            }
                };
                return BadRequest(phoneFormatErrorResponse);
            }
            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Client>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                int defaultRoleId = 3; 

                var role = await _dbContext.Roles.FindAsync(defaultRoleId);

                if (role == null)
                {
                    return BadRequest("Default role not found.");
                }
                var newClient = new Client
                {
                    Name = registrationModel.Name,
                    Username = registrationModel.Username,
                    Password = hashedPassword,
                    Phone = registrationModel.Phone,
                    Address = registrationModel.Address,
                    Avatar = registrationModel.Avatar,
                    Email = registrationModel.Email,
                    Status = registrationModel.Status,
                    CreatedAt = DateTime.Now,
                    CreatedBy = registrationModel.CreatedBy,
                    Role = role,
                };

                _dbContext.Clients.Add(newClient);
                await _dbContext.SaveChangesAsync();

                _dbContext.Entry(newClient).Reference(c => c.Role).Load();

                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    ClientId = newClient.ClientId,
                    Role = new
                    {
                        Name = newClient.Role?.Name,
                        RoleId = newClient.Role?.RoleId
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
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContext.Clients.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

    }
}
