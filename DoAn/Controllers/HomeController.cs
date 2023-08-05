using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var passwordHasher = new PasswordHasher<Cilent>();
                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);

                var newClient = new Cilent
                {
                    Name = registrationModel.Name,
                    Username = registrationModel.Username,
                    Password = hashedPassword, // Store the hashed password
                    Phone = registrationModel.Phone,
                    Address = registrationModel.Address,
                    Avatar = registrationModel.Avatar,
                    Email = registrationModel.Email,
                    // Set other properties
                };

                _dbContext.Cilents.Add(newClient);
                await _dbContext.SaveChangesAsync();

                return Ok("Registration successful");
            }

            return BadRequest("Invalid registration data");
        }
    }
}
