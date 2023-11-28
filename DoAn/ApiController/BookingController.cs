using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;

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
                        Name = registrationModel.Name,
                        Phone = registrationModel.Phone,
                        DateTime = registrationModel.DateTime,
                        Note = registrationModel.Note,
                        Status = registrationModel.Status,
                        CreatedAt = DateTime.Now
                    };

                    _dbContext.Bookings.Add(newBooking);
                    await _dbContext.SaveChangesAsync();
                    //nguoi dung book thi gui mail
                    SendBookingNotificationEmail(staff.Email, registrationModel);

                    if (client != null )
                    {
                        
                        // nguoi dung book thi gui về xác nhận
                        SendBookingConfirmationEmail(client.Email, registrationModel);
                    } 
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

        private async Task SendBookingConfirmationEmail(string clientEmail, Booking registrationModel)
        {
            try
            {
                var clientMessage = new MimeMessage();

                clientMessage.From.Add(new MailboxAddress("Booking Confirmation", "huynhhiepvan1998@gmail.com"));
                clientMessage.Subject = "Booking Confirmation";

                var bookingDate = registrationModel.DateTime?.ToString("yyyy-MM-dd");
                var currentTime = DateTime.Now.ToString("HH:mm");

                var combo = await _dbContext.Combos.FindAsync(registrationModel.ComboId);
                var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);

                clientMessage.Body = new TextPart("html")
                {
                    Text = $@"
                    <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                    <html xmlns=""http://www.w3.org/1999/xhtml"">
                    <head>
                        <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
                        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title></title>
                    </head>
                    <body style=""margin:0;"">
                        <center style=""width: 100%; table-layout: fixed; background-color: #e5dec7;"" class=""wrapper"">
                            <table class=""main"" style=""border-spacing: 0;width: 100%; max-width: 500px; background-color: black; font-family: sans-serif; color: #4a4a4a; box-shadow: 0 0 25px rgba(0, 0, 0, .15);"" width=""100%"">
                                <tr>
                                    <td style=""padding: 5px; background-color: white;"">
                                        <table width=""100%"">
                                            <tr>
                                                <h3 style=""font-family: 'Roboto Condensed', sans-serif; font-size: 28px; text-align: center; color: #c89800;"">CUSTOMER INFORMATION</h3>
                                                <td class=""two-cols"" style=""padding: 0 0 0;"">
                                                    <table style=""width: 100%; max-width: 242px; display: inline-block; vertical-align: top;"" class=""col"">
                                                        <tr>
                                                            <td style=""padding: 0;"" class=""padding"">
                                                                <table style=""border-spacing: 0;font-size: 0;"" class=""content"">
                                                                    <tr>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Name:</strong> {registrationModel.Name}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Phone number:</strong>{registrationModel.Phone}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Booking date: </strong>{bookingDate} / {currentTime}</p>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                       <table style=""border-spacing: 0;width: 100%; max-width: 242px; display: inline-block; vertical-align: top;"" class=""col"">
                                                        <tr>
                                                            <td style=""padding: 0;""  class=""padding"">
                                                                <table style=""border-spacing: 0; font-size: 0;"" class=""content"">
                                                                    <tr>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Notes:</strong> {registrationModel.Note}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Branch:</strong> {branch?.Address}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Combo:</strong> {combo?.Name}</p>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </center>
                    </body>
                    </html>"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    clientMessage.To.Add(new MailboxAddress(clientEmail, clientEmail));

                    await client.SendAsync(clientMessage);

                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }


        //private async Task SendBookingConfirmationEmail(string clientEmail, Booking registrationModel)
        //{
        //    try
        //    {
        //        var clientMessage = new MimeMessage();

        //        clientMessage.From.Add(new MailboxAddress("Booking Confirmation", "huynhhiepvan1998@gmail.com"));
        //        clientMessage.Subject = "Booking Confirmation";

        //        var bookingDate = registrationModel.DateTime?.ToString("yyyy-MM-dd");
        //        var currentTime = DateTime.Now.ToString("HH:mm");

        //        var combo = await _dbContext.Combos.FindAsync(registrationModel.ComboId);
        //        var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);
        //        clientMessage.Body = new TextPart("html")
        //        {
        //            Text = $"<html><body>" +
        //                $"<h2>Your booking has been confirmed!</h2>" +
        //                $"<p><strong>Booking Time: <span style='color: purple'>{bookingDate} / {currentTime}</span></p>" +
        //                $"<p><strong>Note: <span style='color: purple'>{registrationModel.Note}</span></p>" +
        //                $"<p><strong>Branch: <span style='color: purple'>{branch?.Address}</span></p>" +
        //                $"<p><strong>Combo: <span style='color: purple'>{combo?.Name}</span></p>" +
        //                $"</body></html>"
        //        };

        //        using (var client = new SmtpClient())
        //        {
        //            client.Connect("smtp.gmail.com", 587, false);
        //            client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

        //            clientMessage.To.Add(new MailboxAddress(clientEmail, clientEmail));

        //            await client.SendAsync(clientMessage);

        //            client.Disconnect(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error sending email: " + ex.Message);
        //    }
        //}

        private async Task SendBookingNotificationEmail(string staffEmail, Booking registrationModel)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("New Booking", "huynhhiepvan1998@gmail.com"));
                message.Subject = "New Booking Notification";

                var combo = await _dbContext.Combos.FindAsync(registrationModel.ComboId);
                var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);

                var bookingDate = registrationModel.DateTime?.ToString("yyyy-MM-dd");
                var currentTime = DateTime.Now.ToString("HH:mm");

                message.Body = new TextPart("html")
                {
                    Text = $@"
                    <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                    <html xmlns=""http://www.w3.org/1999/xhtml"">
                    <head>
                        <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
                        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title></title>
                    </head>
                    <body style=""margin:0;"">
                        <center style=""width: 100%; table-layout: fixed; background-color: #e5dec7;"" class=""wrapper"">
                            <table class=""main"" style=""border-spacing: 0;width: 100%; max-width: 500px; background-color: black; font-family: sans-serif; color: #4a4a4a; box-shadow: 0 0 25px rgba(0, 0, 0, .15);"" width=""100%"">
                                <tr>
                                    <td style=""padding: 5px; background-color: white;"">
                                        <table width=""100%"">
                                            <tr>
                                                <h3 style=""font-family: 'Roboto Condensed', sans-serif; font-size: 28px; text-align: center; color: #c89800; text-transform: uppercase;"">you have a customer booking</h3>
                                                <td class=""two-cols"" style=""padding: 0 0 0;"">
                                                    <table style=""width: 100%; max-width: 242px; display: inline-block; vertical-align: top;"" class=""col"">
                                                        <tr>
                                                            <td style=""padding: 0;"" class=""padding"">
                                                                <table style=""border-spacing: 0;font-size: 0;"" class=""content"">
                                                                    <tr>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Name:</strong> {registrationModel.Name}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Phone number:</strong>{registrationModel.Phone}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Booking date: </strong>{bookingDate} / {currentTime}</p>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                       <table style=""border-spacing: 0;width: 100%; max-width: 242px; display: inline-block; vertical-align: top;"" class=""col"">
                                                        <tr>
                                                            <td style=""padding: 0;""  class=""padding"">
                                                                <table style=""border-spacing: 0; font-size: 0;"" class=""content"">
                                                                    <tr>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Notes:</strong> {registrationModel.Note}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Branch:</strong> {branch?.Address}</p>
                                                                        <p style=""font-family: 'Roboto Condensed', sans-serif; font-size: 16px; color: #4a4a4a;""><strong>Combo:</strong> {combo?.Name}</p>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </center>
                    </body>
                    </html>"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("huynhhiepvan1998@gmail.com", "nmqt ljyf skbz xcrs");

                    message.To.Add(new MailboxAddress(staffEmail, staffEmail));

                    await client.SendAsync(message);

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
