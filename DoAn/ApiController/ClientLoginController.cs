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
                };
                return BadRequest(loginErrorResponse);
            }

            var passwordHasher = new PasswordHasher<Client>();
            var result = passwordHasher.VerifyHashedPassword(null, client.Password, loginModel.Password);

            if (result == PasswordVerificationResult.Success)
            {
                var loginSuccessResponse = new
                {
                    userID = client.ClientId,
                    Username = client.Username,
                    Name = client.Name,
                    phone = client.Phone,
                    Address = client.Address,
                    Email = client.Email,
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
                    Message = "Không được để trống username password name phone Email đâu Cưng !",
                };
                return BadRequest(emptyFieldsErrorResponse);
            }
            var vietnamesePhoneNumberPattern = @"^(0[0-9]{9,10})$"; 
            if (!Regex.IsMatch(registrationModel.Phone, vietnamesePhoneNumberPattern))
            {
                var phoneFormatErrorResponse = new
                {
                    Message = "Vui lòng nhập đúng định dạng số điện thoại",
                };
                return BadRequest(phoneFormatErrorResponse);
            }
            var emailRegex  = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
            if(!Regex.IsMatch(registrationModel.Email, emailRegex)) 
            {
                var emailEmailFormat = new
                {
                    Message = "Vui lòng nhập đúng định dạng Email",
                };
                return BadRequest(emailEmailFormat);
            }
            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Client>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                int defaultRoleId = 3; 

                var role = await _dbContext.Roles.FindAsync(defaultRoleId);

                if (role == null)
                {
                    return Ok("Default role not found.");
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
        public async Task<IActionResult> GetUserById(int? id)
        {
            if (id == null)
            {
                return Ok(null);
            }
            var user = await _dbContext.Clients.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

    }
}
