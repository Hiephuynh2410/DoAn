using DoAn.Data;
using DoAn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingController : Controller
    {
        private readonly DlctContext _dbContext;
        public BookingController(DlctContext dbContext)
        {
            _dbContext = dbContext;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooking()
        {
            var Booking = await _dbContext.Bookings
                .Include(s => s.Staff)
                .Include(s => s.Client)
                .Include(s => s.Combo)
                .ToListAsync();

            var BookingWithFullInfo = Booking.Select(s => new
            {
                s.BookingId,
                s.Name,
                s.Phone,
                s.DateTime,
                s.Note,
                s.Status,
                s.CreatedAt,
                s.ComboId,
                s.StaffId,
                s.ClientId,
                Staff = new
                {
                    s.Staff.StaffId,
                    s.Staff.Name
                },
                Client = new
                {
                    s.Client.ClientId,
                    s.Client.Name
                },
                Combo = new 
                {
                    s.Combo.ComboId,
                    s.Combo.Name
                },
            }).ToList();

            return Ok(BookingWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] Booking registrationModel)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(registrationModel.Phone) || string.IsNullOrWhiteSpace(registrationModel.Name))
                {
                    var errorResponse = new
                    {
                        Message = "info cannot be empty"
                    };
                    return BadRequest(errorResponse);
                }

                if (ModelState.IsValid)
                {
                    var client = await _dbContext.Clients.FindAsync(registrationModel.ClientId);
                    var staff = await _dbContext.Staff.FindAsync(registrationModel.StaffId);
                    var combo = await _dbContext.Combos.FindAsync(registrationModel.ComboId);

                    if (client == null || staff == null || combo == null)
                    {
                        return NotFound("Client, Staff, or Combo not found.");
                    }

                    var newBooking = new Booking
                    {
                        Client = client,
                        Staff = staff,
                        Combo = combo,
                        Name =registrationModel.Name,
                        Phone = registrationModel.Phone,
                        DateTime = registrationModel.DateTime,
                        Note = registrationModel.Note,
                        Status = registrationModel.Status,
                        CreatedAt = DateTime.Now 
                    };

                    _dbContext.Bookings.Add(newBooking);
                    await _dbContext.SaveChangesAsync();

                    var registrationSuccessResponse = new
                    {
                        Message = "Registration successful",
                        BookingId = newBooking.BookingId,
                        ClientId = newBooking.ClientId,
                        StaffId = newBooking.StaffId,
                        ComboId = newBooking.ComboId,
                        CreatedAt = newBooking.CreatedAt,
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
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
