using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;

namespace DoAn.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly DlctContext _dbContext;

        public HomeController(DlctContext dbContext)
        {
            _dbContext = dbContext;
           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClient()
        {
            var clients = _dbContext.Cilents.ToList();
            return Ok(clients);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var client = await _dbContext.Cilents.FirstOrDefaultAsync(c => c.Username == loginModel.Username);

            if (client == null)
            {
                return BadRequest("Invalid username or password");
            }

            var passwordHasher = new PasswordHasher<Cilent>();
            var result = passwordHasher.VerifyHashedPassword(null, client.Password, loginModel.Password);

            if (result == PasswordVerificationResult.Success)
            {
                return Ok("Login successful");
            }

            return BadRequest("Invalid username or password");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationModel registrationModel)
        {
            if (ModelState.IsValid)
            {
                //kiểm tra email có trùng lặp trong DB không
                if (_dbContext.Cilents.Any(c => c.Email == registrationModel.Email))
                {
                    var emailExistsResponse = new
                    {
                        Message = "Email already exists"
                    };

                    return Conflict(emailExistsResponse); 
                }
                if(string.IsNullOrWhiteSpace(registrationModel.Name) 
                    || string.IsNullOrWhiteSpace(registrationModel.Username) 
                    || string.IsNullOrWhiteSpace(registrationModel.Password) 
                    || string.IsNullOrWhiteSpace(registrationModel.Phone) 
                    || string.IsNullOrWhiteSpace(registrationModel.Email))
                {
                    var nameErrorRespone = new
                    {
                        Message = "Name cannot be empty"
                    };
                    return BadRequest(nameErrorRespone);
                }
                if(!Regex.IsMatch(registrationModel.Phone, @"^(?:\+84|0)\d{9,10}$"))
                {
                    var phoneErrorRespone = new
                    {
                        Message = "Invalid phone number format"
                    };
                    return BadRequest(phoneErrorRespone);
                }
                if(!Regex.IsMatch(registrationModel.Username, "^[]A-Za-z0-9!#$%&'*+/=?^_`{|}~\\,.@()<>[-]*$"))
                {
                    var usernameErroeRespone = new
                    {
                        Message = "Invalid username format,username needs 8 digits and has 1 special character and 1 uppercase character"
                    };
                    return BadRequest(usernameErroeRespone);
                }
                var passwordHasher = new PasswordHasher<Cilent>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                var newClient = new Cilent
                {
                    Name = registrationModel.Name,
                    Username = registrationModel.Username,
                    Password = hashedPassword, 
                    Phone = registrationModel.Phone,
                    Address = registrationModel.Address,
                    Avatar = registrationModel.Avatar,
                    Email = registrationModel.Email,
                };

                _dbContext.Cilents.Add(newClient);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Registration successful",
                    ClientId = newClient.CilentId
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

    }
}
