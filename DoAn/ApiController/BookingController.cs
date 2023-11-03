using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using static System.Net.Mime.MediaTypeNames;
using Org.BouncyCastle.Asn1.Cms;

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
                    s.Staff?.StaffId,
                    s.Staff?.Name
                },
                Client = new
                {
                    s.Client?.ClientId,
                    s.Client?.Name
                },
                Combo = new 
                {
                    s.Combo?.ComboId,
                    s.Combo?.Name
                },
                Branch = new
                {
                    s.Branch?.BranchId,
                    s.Branch?.Address,
                    s.Branch?.Hotline
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

                    if (staff == null || combo == null || branch == null)
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

                    SendBookingNotificationEmail(staff.Email, registrationModel);

                    var registrationSuccessResponse = new
                    {
                        Message = "Registration successful",
                        BookingId = newBooking.BookingId,
                        ClientId = newBooking.ClientId,
                        StaffId = newBooking.StaffId,
                        Staff = new 
                        {
                            Name = newBooking.Staff?.Name,
                            Phone = newBooking.Staff?.Phone,
                        },
                        ComboId = newBooking.ComboId,
                        Combo = new
                        {
                            Address = newBooking.Combo?.Name
                        },
                        BranchId = newBooking.BranchId,
                        Branch = new
                        {
                            Address = newBooking.Branch?.Address,
                            Hotline = newBooking.Branch?.Hotline
                        },
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


        private void SendBookingNotificationEmail(string staffEmail, Booking registrationModel)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("New Booking", "huynhhiepvan1998@gmail.com"));
                message.Subject = "New Booking Notification";

                var comboInfo = registrationModel.Combo != null ? registrationModel.Combo.Name : "N/A";
                var branchInfo = registrationModel.Branch != null ? registrationModel.Branch.Address : "N/A";
                var currentTime = DateTime.Now.ToString("HH:mm");
                message.Body = new TextPart("html")
                {
                    Text = $"<html><body>" +
                       $"<h2>You have a customer booking:</h2>" +
                       $"<p><strong>Client Name:</strong> {registrationModel.Name}</p>" +
                       $"<p><strong>Client Phone:</strong> {registrationModel.Phone}</p>" +
                       $"<p><strong>Time Create:</strong> <span style='color: purple'>{currentTime}</span></p>" +
                       $"<p><strong>Booking Date:</strong> <span style='color: purple'>{registrationModel.DateTime}</span></p>" +
                       $"<p><strong>Note:</strong> <span style='color: purple'>{registrationModel.Note}</span></p>" +
                        $"<p><strong>Branch:</strong> <span style='color: purple'>{branchInfo}</span></p>" +
                        $"<p><strong>Combo:</strong> <span style='color: purple'>{comboInfo}</span></p>" +
                       $"</body></html>"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(staffEmail, staffEmail));

                    client.Send(message);

                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
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
