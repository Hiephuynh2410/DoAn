using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientBookingApiController : Controller
    {
        private readonly DlctContext _dbContext;
        public ClientBookingApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookingFromClient()
        {
            var bookings = await _dbContext.Bookings
                 .Include(b => b.Branch)
                 .Include(b => b.Client)
                 .Include(b => b.Combo)
                 .Include(b => b.Staff)
                 .ToListAsync();

            var bookingFromClientsWithFullInfo = bookings.Select(s => new
            {
                s.BookingId,
                s.ClientId,
                s.StaffId,
                s.ComboId,
                s.Name,
                s.Phone,
                s.DateTime,
                s.Note,
                s.Status,
                s.CreatedAt,
                s.BranchId,
                Branch = s.Branch != null ? new
                {
                    s.Branch.Address,
                    s.Branch.Hotline
                } : null,
                Client = s.Client != null ? new
                {
                    s.Client.Name,
                    s.Client.ClientId
                } : null,
                Combo = s.Combo != null ? new
                {
                    s.Combo.Name,
                    s.Combo.ComboId
                } : null,
                staff = s.Staff != null ? new
                {
                    s.Staff.Name,
                    s.Staff.StaffId
                } : null
            }).ToList();

            return Ok(bookingFromClientsWithFullInfo);
        }

        [HttpPut("update/{bookingId}")]
        public async Task<IActionResult> UpdateBookingFromClient(int bookingId)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return NotFound();
            }
            booking.Status = false;
            booking.IsBooking = false;
            _dbContext.Entry(booking).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Booking status updated successfully"
            };

            return Ok(updateSuccessResponse);
        }
    }
}
