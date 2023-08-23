//using DoAn.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Text.RegularExpressions;

//namespace DoAn.ApiController
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ClientLoginController : Controller
//    {
//        private readonly DlctContext _dbContext;
//        public ClientLoginController(DlctContext dbContext)
//        {
//            _dbContext = dbContext;

//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAllClient()
//        {
//           var clients = await _dbContext.Clients
//                .Include(c => c.Role)
//                .ToListAsync();

//            var ClientWithFullInfo = clients.Select(c => new
//            {
//                c.ClientId,
//                c.Name,
//                c.Username,
//                c.Password,
//                c.Phone,
//                c.Avatar,
//                c.Address,
//                c.Email,
//                c.Status,
//                c.RoleId,
//                Role = new
//                {
//                    c.Role.RoleId,
//                    c.Role.Name
//                },
//                c.CreatedAt, 
//                c.UpdatedAt,
//                c.CreatedBy,
//                c.UpdatedBy,
//                c.Bookings
//            }).ToList();
//            return Ok(ClientWithFullInfo);
//        }

//        [HttpGet("login")]
//        public async Task<IActionResult> Login(Cilent loginModel)
//        {
//            var client = await _dbContext.Cilents.FirstOrDefaultAsync(c => c.Username == loginModel.Username);
//            if (client == null)
//            {
//                var loginErrorResponse = new
//                {
//                    Message = "Invalid username or password",
//                    Errors = new List<string>
//                    {
//                        "Invalid username"
//                    }
//                };

//                return BadRequest(loginErrorResponse);
//            }
//            if (string.IsNullOrWhiteSpace(loginModel.Username) && string.IsNullOrWhiteSpace(loginModel.Password))
//            {
//                var nameErrorRespone = new
//                {
//                    Message = "Name and password cannot be empty"
//                };
//                return BadRequest(nameErrorRespone);
//            }

//            var passwordHasher = new PasswordHasher<Cilent>();
//            var result = passwordHasher.VerifyHashedPassword(null, client.Password, loginModel.Password);
//            if (result == PasswordVerificationResult.Success)
//            {
//                var loginSuccessResponse = new
//                {
//                    Message = "Login successful"
//                };

//                return Ok(loginSuccessResponse);
//            }

//            var invalidLoginErrorResponse = new
//            {
//                Message = "Invalid username or password",
//                Errors = new List<string>
//                {
//                    "Invalid password"
//                }
//            };

//            return BadRequest(invalidLoginErrorResponse);
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register(Cilent registrationModel)
//        {
//            if (ModelState.IsValid)
//            {
//                //kiểm tra email có trùng lặp trong DB không
//                if (_dbContext.Clients.Any(c => c.Email == registrationModel.Email))
//                {
//                    var emailExistsResponse = new
//                    {
//                        Message = "Email already exists"
//                    };


//                    return Conflict(emailExistsResponse);
//                }
//                if (string.IsNullOrWhiteSpace(registrationModel.Name)
//                    || string.IsNullOrWhiteSpace(registrationModel.Username)
//                    || string.IsNullOrWhiteSpace(registrationModel.Password)
//                    || string.IsNullOrWhiteSpace(registrationModel.Phone)
//                    || string.IsNullOrWhiteSpace(registrationModel.Email))
//                {
//                    var nameErrorRespone = new
//                    {
//                        Message = "Name, username, password, phone, and email cannot be empty"
//                    };
//                    return BadRequest(nameErrorRespone);
//                }
//                if (registrationModel.Name.Length > 100)
//                {
//                    var nameErrorRespone = new
//                    {
//                        Message = "Name is too long"
//                    };
//                    return BadRequest(nameErrorRespone);
//                }
//                if (!Regex.IsMatch(registrationModel.Phone, @"^(?:\+84|0)\d{9,10}$"))
//                {
//                    var phoneErrorRespone = new
//                    {
//                        Message = "Invalid phone number format"
//                    };
//                    return BadRequest(phoneErrorRespone);
//                }
//                if (!Regex.IsMatch(registrationModel.Username, "^[]A-Za-z0-9!#$%&'*+/=?^_`{|}~\\,.@()<>[-]*$"))
//                {
//                    var usernameErroeRespone = new
//                    {
//                        Message = "Invalid username format,username needs 8 digits and has 1 special character and 1 uppercase character"
//                    };
//                    return BadRequest(usernameErroeRespone);
//                }
//                var passwordHasher = new PasswordHasher<Client>();
//                var hashedPassword = passwordHasher.HashPassword(null, registrationModel.Password);
//                var Role = await _dbContext.Roles.FindAsync(registrationModel.RoleId);
//                var newClient = new Client
//                {
//                    Name = registrationModel.Name,
//                    Username = registrationModel.Username,
//                    Password = hashedPassword,
//                    Phone = registrationModel.Phone,
//                    Address = registrationModel.Address,
//                    Avatar = registrationModel.Avatar,
//                    Email = registrationModel.Email,
//                    Role = Role,
//                };

//                _dbContext.Clients.Add(newClient);
//                await _dbContext.SaveChangesAsync();
//                _dbContext.Entry(newClient).Reference(s => s.Role).Load();

//                var registrationSuccessResponse = new
//                {
//                    Message = "Registration successful",
//                    ClientId = newClient.ClientId,
//                    Role = new
//                    {
//                        Name = newClient.Role?.Name,
//                        RoleId = newClient.Role?.RoleId
//                    }
//                };
//                return Ok(registrationSuccessResponse);
//            }

//            var invalidDataErrorResponse = new
//            {
//                Message = "Invalid registration data",
//                Errors = ModelState.Values
//                    .SelectMany(v => v.Errors)
//                    .Select(e => e.ErrorMessage)
//                    .ToList()
//            };
//            return BadRequest(invalidDataErrorResponse);
//        }

//        [HttpPut("update/{clientId}")]
//        public async Task<IActionResult> UpdateClient(int clientId, Cilent updateModel)
//        {
//            var client = await _dbContext.Cilents.FindAsync(clientId);

//            if (client == null)
//            {
//                return NotFound();
//            }

//            if (!string.IsNullOrWhiteSpace(updateModel.Name))
//            {
//                client.Name = updateModel.Name;
//            }
//            if (!string.IsNullOrWhiteSpace(updateModel.Username))
//            {
//                client.Username = updateModel.Username;
//            }
//            if (!string.IsNullOrWhiteSpace(updateModel.Phone))
//            {
//                client.Phone = updateModel.Phone;
//            }
//            if (!string.IsNullOrWhiteSpace(updateModel.Address))
//            {
//                client.Address = updateModel.Address;
//            }
//            if (!string.IsNullOrWhiteSpace(updateModel.Avatar))
//            {
//                client.Avatar = updateModel.Avatar;
//            }
//            if (!string.IsNullOrWhiteSpace(updateModel.Email))
//            {
//                //kiểm tra email có trùng lặp trong DB không
//                if (_dbContext.Cilents.Any(c => c.Email == updateModel.Email && c.CilentId != clientId))
//                {
//                    var emailExistsResponse = new
//                    {
//                        Message = "Email already exists"
//                    };

//                    return Conflict(emailExistsResponse);
//                }

//                client.Email = updateModel.Email;
//            }

//            _dbContext.Entry(client).State = EntityState.Modified;
//            await _dbContext.SaveChangesAsync();

//            var updateSuccessResponse = new
//            {
//                Message = "Client updated successfully"
//            };

//            return Ok(updateSuccessResponse);
//        }

//        [HttpDelete("delete/{clientId}")]
//        public async Task<IActionResult> DeleteClient(int clientId)
//        {
//            var client = await _dbContext.Cilents.FindAsync(clientId);

//            if (client == null)
//            {
//                return NotFound();
//            }

//            _dbContext.Cilents.Remove(client);
//            await _dbContext.SaveChangesAsync();

//            var deleteSuccessResponse = new
//            {
//                Message = "Client deleted successfully"
//            };

//            return Ok(deleteSuccessResponse);
//        }

//        [HttpGet("detail/{clientId}")]
//        public async Task<IActionResult> GetClientDetail(int clientId)
//        {
//            var client = await _dbContext.Clients.FindAsync(clientId);

//            if (client == null)
//            {
//                return NotFound();
//            }

//            var clientDetail = new
//            {
//                client.CilentId,
//                client.Name,
//                client.Username,
//                client.Phone,
//                client.Address,
//                client.Avatar,
//                client.Email,
//                client.Status,
//                client.CreatedAt,
//                client.UpdatedAt,
//                client.CreatedBy,
//                client.UpdatedBy
//            };

//            return Json(clientDetail); 
//        }
//    }
//}
