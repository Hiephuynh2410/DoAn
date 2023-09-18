//using DoAn.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace DoAn.ApiController
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class BookingController : Controller
//    {
//        private readonly DlctContext _dbContext;
//        public BookingController(DlctContext dbContext)
//        {
//            _dbContext = dbContext;

//        }

//        public async Task<IActionResult> RegisterStaff(Booking registrationModel)
//        {
//            if (ModelState.IsValid)
//            {
                
//                var Client = await _dbContext.Clients.FindAsync(registrationModel.Client);
//                var Staff = await _dbContext.Staff.FindAsync(registrationModel.Staff);
//                var combo = await _dbContext.Combos.FindAsync(registrationModel.Combo);

//                var newStaff = new Staff
//                {
//                    Name = registrationModel.Name,
//                    Username = registrationModel.Username,
//                    Password = hashedPassword,
//                    Phone = registrationModel.Phone,
//                    Address = registrationModel.Address,
//                    Avatar = registrationModel.Avatar,
//                    Email = registrationModel.Email,
//                    Status = registrationModel.Status,
//                    CreatedAt = DateTime.Now,
//                    CreatedBy = registrationModel.CreatedBy,
//                    Branch = branch,
//                    Role = role,
//                };

//                _dbContext.Staff.Add(newStaff);
//                await _dbContext.SaveChangesAsync();

//                _dbContext.Entry(newStaff).Reference(s => s.Branch).Load();
//                _dbContext.Entry(newStaff).Reference(s => s.Role).Load();

//                var registrationSuccessResponse = new
//                {
//                    Message = "Registration successful",
//                    ClientId = newStaff.StaffId,
//                    Branch = new
//                    {
//                        Address = newStaff.Branch?.Address,
//                        Hotline = newStaff.Branch?.Hotline
//                    },
//                    Role = new
//                    {
//                        Name = newStaff.Role?.Name,
//                        RoleId = newStaff.Role?.RoleId
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
//    }
//}
