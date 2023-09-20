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
                .Include(s => s.Branch)
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
                Branch = new
                {
                    s.Branch.BranchId,
                    s.Branch.Address,
                    s.Branch.Hotline
                }
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
                    var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);

                    if (client == null || staff == null || combo == null || branch == null)
                    {
                        return NotFound("Client, Staff, booking, or Combo not found.");
                    }

                    var newBooking = new Booking
                    {
                        Client = client,
                        Staff = staff,
                        Combo = combo,
                        Branch = branch,
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
                        BranchId = newBooking.BranchId,
                        Name = registrationModel.Name,
                        Phone = registrationModel.Phone,
                        DateTime = registrationModel.DateTime,
                        Note = registrationModel.Note,
                        Status = registrationModel.Status,
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

        [HttpPut("update/{bookingId}")]
        public async Task<IActionResult> UpdateBookingClient(int bookingId, Booking updateModel)
        {
            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Name = updateModel.Name;
            booking.Phone = updateModel.Phone;
            booking.Note= updateModel.Note;
            booking.Status = updateModel.Status;
            booking.DateTime = DateTime.Now;

            if (updateModel.BranchId != booking.BranchId)
            {
                var newBranch = await _dbContext.Branches.FindAsync(updateModel.BranchId);
                if (newBranch != null)
                {
                    booking.Branch = newBranch;
                }
            }

            if (updateModel.ClientId != booking.ClientId)
            {
                var newClient = await _dbContext.Clients.FindAsync(updateModel.ClientId);
                if (newClient != null)
                {
                    booking.Client = newClient;
                }
            }

            if (updateModel.ComboId != booking.ComboId)
            {
                var newCombo = await _dbContext.Combos.FindAsync(updateModel.ComboId);
                if (newCombo != null)
                {
                    booking.Combo = newCombo;
                }
            }

            if (updateModel.StaffId != booking.StaffId)
            {
                var newStaff = await _dbContext.Staff.FindAsync(updateModel.StaffId);
                if (newStaff != null)
                {
                    booking.Staff = newStaff;
                }
            }

            _dbContext.Entry(booking).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "booking updated successfully"
            };

            return Ok(updateSuccessResponse);
        }
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var branches = await _dbContext.Branches.ToListAsync();

                var branchData = branches.Select(branch => new
                {
                    BranchId = branch.BranchId,
                    Address = branch.Address,
                    Hotline = branch.Hotline
                }).ToList();

                return Ok(branchData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
