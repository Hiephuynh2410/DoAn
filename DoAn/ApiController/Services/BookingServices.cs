using DoAn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using DoAn.ApiController.Mail;

namespace DoAn.ApiController.Services
{
    public class BookingServices
    {
        private readonly DlctContext _dbContext;
        private readonly SendMail _sendMail;

        public BookingServices(DlctContext dbContext, SendMail sendMail)
        {
            _dbContext = dbContext;
            _sendMail = sendMail;
        }
        //khi ông test thì ông sẽ 
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

                    return new BadRequestObjectResult(errorResponse);
                }

                var client = await _dbContext.Clients.FindAsync(registrationModel.ClientId);
                var staff = await _dbContext.Staff.FindAsync(registrationModel.StaffId);
                var combo = await _dbContext.Combos.FindAsync(registrationModel.ComboId);
                var branch = await _dbContext.Branches.FindAsync(registrationModel.BranchId);

                if (staff == null || combo == null || branch == null)
                {
                    var Check = new List<string>();
                    if (staff == null) Check.Add("Staff");
                    if (combo == null) Check.Add("Combo");
                    if (branch == null) Check.Add("Branch");

                    return new NotFoundObjectResult($"Thieu tp nay: {string.Join(", ", Check)}.");
                }

                var existingBooking = await _dbContext.Bookings
                    .FirstOrDefaultAsync(b => b.StaffId == registrationModel.StaffId &&
                                              b.DateTime == registrationModel.DateTime);



                if (existingBooking != null)
                {
                    if (registrationModel.IsBooking == false) // nếu isBoooking == false thì cho phép user book tiếp nhân viên đó

                    {
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
                        _sendMail.SendBookingNotificationEmail(staff.Email, registrationModel);


                        if (client != null)
                        {
                            _sendMail.SendBookingConfirmationEmail(client.Email, registrationModel);
                        }
                        var registrationSuccessResponse = new
                        {
                            Message = "Registration successful",
                            BookingId = newBooking.BookingId,
                            ClientId = newBooking.ClientId,
                            Staff = new
                            {
                                StaffId = newBooking.Staff?.StaffId,
                                Name = newBooking.Staff?.Name,
                                Phone = newBooking.Staff?.Phone,
                            },
                            Combo = new
                            {
                                ComboId = newBooking.Combo?.ComboId,
                                Address = newBooking.Combo?.Name
                            },
                            Branch = new
                            {
                                BranchId = newBooking.Branch?.BranchId,
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
                    
                        return new OkObjectResult(registrationSuccessResponse);
                    }
                    else
                    {
                        return new BadRequestObjectResult("Nhân viên đã được đặt vào thời gian này.");
                    }
                }
                else
                {
                    var scheduleDetails = await _dbContext.Scheduledetails
                                .AnyAsync(sd => sd.StaffId == registrationModel.StaffId &&
                               sd.Date == registrationModel.DateTime);

                    if (scheduleDetails)
                    {
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
                        _sendMail.SendBookingNotificationEmail(staff.Email, registrationModel);


                        if (client != null)
                        {
                            _sendMail.SendBookingConfirmationEmail(client.Email, registrationModel);
                        }
                        var registrationSuccessResponse = new
                        {
                            Message = "Registration successful",
                            BookingId = newBooking.BookingId,
                            ClientId = newBooking.ClientId,
                            Staff = new
                            {
                                StaffId = newBooking.Staff?.StaffId,
                                Name = newBooking.Staff?.Name,
                                Phone = newBooking.Staff?.Phone,
                            },
                            Combo = new
                            {
                                ComboId = newBooking.Combo?.ComboId,
                                Address = newBooking.Combo?.Name
                            },
                            Branch = new
                            {
                                BranchId = newBooking.Branch?.BranchId,
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

                        return new OkObjectResult(registrationSuccessResponse);
                    }
                    else
                    {
                        return new BadRequestObjectResult("Nhân viên không có lịch làm việc vào thời gian này.");
                    }
                }
              
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred: {ex.Message}") { StatusCode = 500 };
            }
        }

        public async Task<List<object>> GetAllBooking()
        {
            var AllBookings = await _dbContext.Bookings
                .Include(s => s.Staff)
                .Include(s => s.Client)
                .Include(s => s.Combo)
                .Include(s => s.Branch)
                .ToListAsync();
            return AllBookings.Select(s => new
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
            }).Cast<object>().ToList();
        }

        public async Task<IActionResult> UpdateBookingClient(int bookingId, Booking updateModel)
        {
            var booking = await _dbContext.Bookings
               .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (booking == null)
            {
                return new NotFoundResult();
            }

            booking.Name = updateModel.Name;
            booking.Phone = updateModel.Phone;
            booking.Note = updateModel.Note;
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

            return new OkObjectResult(updateSuccessResponse);
        }

        public async Task<List<object>> GetBranches()
        {
            var branches = await _dbContext.Branches.ToListAsync();

            return branches.Select(s => new
            {
                BranchId = s.BranchId,
                Address = s.Address,
                Hotline = s.Hotline
            }).Cast<object>().ToList();
        }
    }
}
//_sendMail.SendBookingNotificationEmail(staff.Email, registrationModel);


//if (client != null)
//{
//    _sendMail.SendBookingConfirmationEmail(client.Email, registrationModel);
//}